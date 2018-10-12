Imports Stylet
Imports ECMod.ViewModels
Imports System.Text.RegularExpressions

Partial Public Class RootViewModel : Inherits Conductor(Of Screen)

    Public Property Toolbar As ToolbarViewModel
    Public Property Monitor As MonitorViewModel
    Public Property Working As WorkingViewModel
    Public Property SensorEvent As SensorEvent

    Public windowManager As IWindowManager

    Sub New(windowManager As IWindowManager)

        Me.windowManager = windowManager
        CheckSettings()
        BaseTimer.Interval = SettingsXML.<config>.<Global_Settings>.<RefreshInt>.Value
        BaseTimer.Start()
        AddHandler BaseTimer.Elapsed, AddressOf DoBaseTick

        ProcessArgs = RW_ProcessArgumentBuilder()

        Toolbar = New ToolbarViewModel(windowManager)
        Monitor = New MonitorViewModel(eventAggregator)
        Working = New WorkingViewModel
        SensorEvent = New SensorEvent(SettingsXML.<config>.<P650RE>.First)

    End Sub

    Dim RWOutputVals As New Dictionary(Of String, Integer)

    Sub DoBaseTick()
        SensorEvent.GPU_Temp = GetGPUTemp()
        eventAggregator.Publish(SensorEvent)
        IsMonitoringActive = True
        ParseRWData()
    End Sub

    Async Sub ParseRWData()
        Dim Raw = Await RW_DataOut()
        Dim RGX As String = "0x(\w|\d){2} = 0x(\w|\d){2}(\r|\n|$)"
        RWOutputVals.Clear()

        For Each mx As Match In Regex.Matches(Raw, RGX)
            Dim key = mx.Value.Split("=")(0).Trim()
            Dim val = VBHex(mx.Value.Split("=")(1).Trim())
            RWOutputVals.Add(key, val)
            IsMonitoringActive = False
        Next
        SensorEvent.Populate(RWOutputVals)
    End Sub

    Dim RWProc As Process
    Dim ProcessArgs As String

    Async Function RW_DataOut() As Task(Of String)
        RWProc = New Process
        With RWProc.StartInfo
            .FileName = RW
            .Arguments = ProcessArgs
            .UseShellExecute = False
            .CreateNoWindow = True
            .RedirectStandardInput = True
            .RedirectStandardOutput = True
            .RedirectStandardError = True
        End With
        RWProc.Start()

        Dim t = Await RWProc.StandardOutput.ReadToEndAsync
        RWProc.Close()
        Return (t)

    End Function

    Function RW_ProcessArgumentBuilder() As String

        Dim ArgBuilder As New Text.StringBuilder("/stdout /nologo /Command=")

        For Each offset In SettingsXML.<config>.<P650RE>.<Offsets>.Elements
            If offset.Value.Contains(",") Then
                For Each suboffset In offset.Value.Split(",")
                    ArgBuilder.AppendFormat("""REC {0}"";", suboffset)
                Next
            Else
                ArgBuilder.AppendFormat("""REC {0}"";", offset.Value)
            End If
        Next
        Return ArgBuilder.ToString
    End Function

    Sub CheckSettings()
        If IO.File.Exists("Settings.xml") = False Then
            Dim BackupSettings As XDocument =
                <?xml version="1.0" encoding="utf-8"?>
                <config>
                    <Global_Settings>
                        <RefreshInt>1000</RefreshInt>
                        <RWEverythingPath></RWEverythingPath>
                    </Global_Settings>
                    <P650RE>
                        <Offsets>
                            <CPU_Temp>0x07</CPU_Temp>
                            <CPU_Fan_Duty>0xCE</CPU_Fan_Duty>
                            <GPU_Fan_Duty>0xCF</GPU_Fan_Duty>
                            <CPU_Fan_RPM1>0xD0,0xD1</CPU_Fan_RPM1>
                            <GPU_Fan_RPM1>0xD2,0xD3</GPU_Fan_RPM1>
                            <GPU_Fan_RPM2>0xD4,0xD5</GPU_Fan_RPM2>
                            <GPU_Fan_Target>0xE7</GPU_Fan_Target>
                        </Offsets>
                        <Functions>
                            <FanRPM>32768*60/[n]</FanRPM>
                            <TargetMaxRPM>30*[n]+3660</TargetMaxRPM>
                            <TargetIdleRPM>-0.0016*(([n]+127)^3)+0.848*(([n]+127)^2)-116.6*([n]+127)+5960</TargetIdleRPM>
                        </Functions>
                    </P650RE>
                </config>

            BackupSettings.Save("Settings.xml")
        End If

        SettingsXML = XDocument.Load("Settings.xml")
        Dim RWPath = SettingsXML.<config>.<Global_Settings>.<RWEverythingPath>.Single.Value

        If RWPath IsNot "" AndAlso IO.File.Exists(RWPath) Then
            RW = RWPath
        Else
            MsgBox("RW.exe was not found. Please browse to its location and select it to continue.")
            Dim fdlg As New Microsoft.Win32.OpenFileDialog
            With fdlg
                .DefaultExt = ".exe"
                .Filter = "Executables (*.exe)|*.exe"
                .Title = "Please find RW Everything executable (RW.exe)"
            End With
            fdlg.ShowDialog()

            If fdlg.FileName IsNot "" Then
                SettingsXML.<config>.<Global_Settings>.<RWEverythingPath>.Single.Value = fdlg.FileName
                SettingsXML.Save("Settings.xml")
                CheckSettings()
            Else
                Application.Current.Shutdown()
            End If


        End If

    End Sub









End Class



