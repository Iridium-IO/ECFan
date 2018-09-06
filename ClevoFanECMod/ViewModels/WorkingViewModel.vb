Imports Stylet
Imports org.mariuszgromada.math.mxparser
Imports System.Text.RegularExpressions

Namespace ViewModels
    Public Class WorkingViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)

        Property Current_Offset As Integer = 0

        Property Current_Idle As String = "0 RPM"
        Property Current_Max As String = "0 RPM"

        Property New_Idle As String = "20 RPM"
        Property New_Max As String = "40 RPM"

        Private _Offset_Slider As Integer = 0
        Property Offset_Slider As Integer
            Get
                Return _Offset_Slider
            End Get
            Set(value As Integer)
                DoMath(value, New_Idle, New_Max)
                _Offset_Slider = value
            End Set
        End Property

        Dim XMLFunctions As XElement = SettingsXML.<config>.<P650RE>.<Functions>.Single
        Dim WriteableOffset As String = SettingsXML.<config>.<P650RE>.<Offsets>.<GPU_Fan_Target>.Value

        Sub New()
            eventAggregator.Subscribe(Me)
        End Sub

        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle
            If Current_Idle = "0 RPM" OrElse Current_Offset <> message.GPU_Fan_Target Then
                Current_Offset = message.GPU_Fan_Target
                DoMath(message.GPU_Fan_Target, Current_Idle, Current_Max)
            End If
        End Sub


        Dim WriteableVal As String = "0x00"

        Private Sub DoMath(value As Integer, ByRef prop_Idle As String, ByRef prop_Max As String)

            Dim idle_rgx = Regex.Replace(XMLFunctions.<TargetIdleRPM>.Value, "\[n\]", value.ToString)
            Dim max_rgx = Regex.Replace(XMLFunctions.<TargetMaxRPM>.Value, "\[n\]", value.Clamp(0, 38).ToString)
            Dim idle_calc As Expression = New Expression(idle_rgx)
            Dim max_calc As Expression = New Expression(max_rgx)

            prop_Idle = (idle_calc.calculate \ 10) * 10 & " RPM"
            prop_Max = (max_calc.calculate \ 10) * 10 & " RPM"
            WriteableVal = "0x" & Hex(value)

        End Sub

        Async Sub SubmitNewFanOffset()
            Dim xas As String = String.Format("/stdout /nologo /Command=""WEC {0} {1}""", WriteableOffset, WriteableVal)

            Debug.WriteLine(xas)

            If MsgBox(String.Format("Are you sure you want to write {0} to the Embedded Controller at address {1}?", WriteableVal, WriteableOffset), MsgBoxStyle.YesNo, "Caution!") = MsgBoxResult.Yes Then
                BaseTimer.Enabled = False
                Dim result = Await RW_DataWrite(xas)
                If result IsNot Nothing Then
                    Debug.WriteLine(result)
                    BaseTimer.Enabled = True
                Else
                    BaseTimer.Enabled = True
                End If

            Else
                Debug.WriteLine("Hell naw")
            End If

        End Sub


        Dim RWProc As Process


        Async Function RW_DataWrite(arguments As String) As Task(Of String)
            RWProc = New Process
            With RWProc.StartInfo
                .FileName = RW
                .Arguments = arguments
                .UseShellExecute = False
                .CreateNoWindow = True
                .RedirectStandardInput = True
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With
            RWProc.Start()
            Dim t = Await RWProc.StandardOutput.ReadToEndAsync
            Return t
        End Function



    End Class
End Namespace
