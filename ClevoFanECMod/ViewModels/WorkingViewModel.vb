Imports Stylet
Imports org.mariuszgromada.math.mxparser
Imports System.Text.RegularExpressions
Imports LiveCharts
Imports LiveCharts.Defaults


Namespace ViewModels
    Public Class WorkingViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)

        Property Current_Offset As Integer = 0
        Property Current_Idle As Integer = 0
        Property Current_Max As Integer = 0
        Property New_Idle As Integer = 0
        Property New_Max As Integer = 0
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
        Dim WriteableVal As String = "0x00"

        Sub New()
            eventAggregator.Subscribe(Me)
            DoMath(0, New_Idle, New_Max)

        End Sub

        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle
            If Current_Idle = 0 OrElse Current_Offset <> message.GPU_Fan_Target Then
                Current_Offset = message.GPU_Fan_Target
                DoMath(message.GPU_Fan_Target, Current_Idle, Current_Max)

                Chart_CurrentValues = New ChartValues(Of ObservablePoint) From {
                    New ObservablePoint(-1, Current_Idle),
                    New ObservablePoint(RescaleVal(Current_Idle, False), Current_Idle),
                    New ObservablePoint(RescaleVal(Current_Max, True), Current_Max),
                    New ObservablePoint(4, Current_Max)}


            End If
        End Sub



        Private Sub DoMath(value As Integer, ByRef prop_Idle As String, ByRef prop_Max As String)

            Dim idle_rgx = Regex.Replace(XMLFunctions.<TargetIdleRPM>.Value, "\[n\]", value.ToString)
            Dim max_rgx = Regex.Replace(XMLFunctions.<TargetMaxRPM>.Value, "\[n\]", value.Clamp(0, 38).ToString)
            Dim idle_calc As Expression = New Expression(idle_rgx)
            Dim max_calc As Expression = New Expression(max_rgx)
            WriteableVal = "0x" & Hex(value)
            prop_Idle = (idle_calc.calculate \ 10) * 10
            prop_Max = (max_calc.calculate \ 10) * 10

            Execute.PostToUIThreadAsync(New Action(Sub() UpdateCharts()))

        End Sub


        Property Chart_NewValues = New ChartValues(Of ObservablePoint)
        Property Chart_CurrentValues = New ChartValues(Of ObservablePoint)


        Sub UpdateCharts()

            If Chart_NewValues.Count = 4 Then

                Chart_NewValues(0) = New ObservablePoint(-1, New_Idle)
                Chart_NewValues(1) = New ObservablePoint(RescaleVal(New_Idle, False), New_Idle)
                Chart_NewValues(2) = New ObservablePoint(RescaleVal(New_Max, True), New_Max)
                Chart_NewValues(3) = New ObservablePoint(4, New_Max)

            Else
                Chart_NewValues = New ChartValues(Of ObservablePoint) From {
                    New ObservablePoint(-1, New_Idle),
                    New ObservablePoint(RescaleVal(New_Idle, False), New_Idle),
                    New ObservablePoint(RescaleVal(New_Max, True), New_Max),
                    New ObservablePoint(4, New_Max)}
            End If



        End Sub


        Function RescaleVal(value As Integer, ismaxnotidle As Boolean) As Double

            Dim min As Integer = 1550
            Dim max As Integer = 4800
            Dim scale_lower As Double = 1
            Dim scale_upper As Double = 0.5

            If ismaxnotidle Then
                min = 3660
                max = 4800
                scale_lower = 2.2
                scale_upper = 1.95
            End If

            Return ((scale_upper - scale_lower) * (value - min)) / (max - min) + scale_lower


        End Function







        Async Sub SubmitNewFanOffset()
            Dim constructed_arg As String = String.Format("/stdout /nologo /Command=""WEC {0} {1}""", WriteableOffset, WriteableVal)

            If MsgBox(String.Format("Are you sure you want to write {0} to the Embedded Controller at address {1}?", WriteableVal, WriteableOffset), MsgBoxStyle.YesNo, "Caution!") = MsgBoxResult.Yes Then

                While IsMonitoringActive = True
                    'Do Nothing
                End While

                Dim result = Await RW_DataWrite(constructed_arg)
                If result.Contains(WriteableVal) Then
                    Debug.WriteLine("Success")
                    BaseTimer.Enabled = True
                Else
                    Debug.WriteLine("Failure")
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
            RWProc.Close()
            Return t
        End Function

    End Class
End Namespace
