Imports Stylet
Imports ClevoFanECMod.ViewModels
Imports org.mariuszgromada.math.mxparser
Imports System.Text.RegularExpressions


Partial Public Class RootViewModel : Inherits Conductor(Of Screen)

    Public Property Toolbar As New ToolbarViewModel
    Public Property Monitor As New MonitorViewModel(eventAggregator)
    Public Property Working As New WorkingViewModel

    Dim _SensorEvent As New SensorEvent(SettingsXML.<config>.<P650RE>.First)


    Sub New()

        BaseTimer.Start()

        ProcessArgs = RW_ProcessArgumentBuilder()

        AddHandler BaseTimer.Elapsed, AddressOf DoBaseTick

    End Sub

    Dim TargetIdleRPM = SettingsXML.<config>.<P650RE>.<Functions>.<TargetIdleRPM>.Value
    Dim RWOutputVals As New Dictionary(Of String, Integer)

    Sub DoBaseTick()
        _SensorEvent.GPU_Temp = GetGPUTemp()
        eventAggregator.Publish(_SensorEvent)
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
        _SensorEvent.Populate(RWOutputVals)

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


End Class



