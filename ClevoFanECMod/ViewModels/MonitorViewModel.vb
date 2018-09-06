Imports Stylet
Imports LiveCharts
Imports LiveCharts.Wpf
Imports ClevoFanECMod.Models
Imports LiveCharts.Configurations

Namespace ViewModels

    Public Class MonitorViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)

        Property MonitorsCollection As BindableCollection(Of Monitor_Single)

        Sub New(ByRef eventAggregator As IEventAggregator)
            eventAggregator.Subscribe(Me)

            Dim mapper = Mappers.
                Xy(Of MeasureModel)().
                X(Function(x) x.datetime.Ticks).
                Y(Function(y) y.value).
                Stroke(Function(b) Brushes.Transparent).
                Fill(Function(b) Brushes.White)

            Charting.For(Of MeasureModel)(mapper)

            SeriesCollection = New SeriesCollection From {CPUTemp, GPUTemp, CPURPMSpeed, GPURPMSpeed1}
            TemperatureCollection = New SeriesCollection From {CPUTemp, GPUTemp}
            RPMCollection = New SeriesCollection From {CPURPMSpeed, GPURPMSpeed1}

            SetAxisLimits(DateTime.Now)

            Grad1.Freeze()
            Grad2.Freeze()

        End Sub

        Public Property SeriesCollection As SeriesCollection
        Public Property TemperatureCollection As SeriesCollection
        Public Property RPMCollection As SeriesCollection

        Private Property CPUTemp As New LineSeries With {.Title = "CPU", .LabelPoint = Function(x) (x.Y & "°C"), .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 7, .Stroke = Grad1}
        Private Property GPUTemp As New LineSeries With {.Title = "GPU", .LabelPoint = Function(x) (x.Y & "°C"), .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 7, .Stroke = Grad2}

        Private Property CPURPMSpeed As New LineSeries With {.Title = "CPU", .LabelPoint = Function(x) (x.Y & " RPM"), .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 7, .Stroke = Grad1}
        Private Property GPURPMSpeed1 As New LineSeries With {.Title = "GPU", .LabelPoint = Function(x) (x.Y & " RPM"), .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 7, .Stroke = Grad2}

        Public Property DateTimeFormatter As Func(Of Double, String) = Function(x) (New DateTime(CLng(x)).ToString("HH:mm:ss"))
        Public Property AxisStep As Double = TimeSpan.FromSeconds(21).Ticks
        Public Property AxisUnit As Double = TimeSpan.TicksPerSecond
        Public Property AxisMax As Double
        Public Property AxisMin As Double



        Private Sub SetAxisLimits(now As DateTime)
            AxisMax = now.Ticks
            AxisMin = now.Ticks - TimeSpan.FromSeconds(20).Ticks
        End Sub


        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle
            Dim xact As New Action(Sub() DoRead(message))
            Execute.PostToUIThreadAsync(xact)
        End Sub

        Private Sub DoRead(message As SensorEvent)

            Dim now = DateTime.Now
            CPUTemp.Values.Add(New MeasureModel With {.datetime = now, .value = CInt(message.CPU_Temp)})
            GPUTemp.Values.Add(New MeasureModel With {.datetime = now, .value = CInt(message.GPU_Temp)})
            CPURPMSpeed.Values.Add(New MeasureModel With {.datetime = now, .value = RPMSmoother(message.CPU_Fan_RPM1)})
            GPURPMSpeed1.Values.Add(New MeasureModel With {.datetime = now, .value = RPMSmoother(message.GPU_Fan_RPM1)})

            If CPUTemp.Values.Count > 23 Then
                For Each i In SeriesCollection
                    i.Values.RemoveAt(0)
                Next
            End If

            SetAxisLimits(now)


            Dim tempMonitor As New BindableCollection(Of Monitor_Single)

            With tempMonitor
                .Add(New Monitor_Single("CPU Fan: ", message.CPU_Fan_RPM1, " RPM", Grad1))
                .Add(New Monitor_Single("GPU Fan: ", message.GPU_Fan_RPM1, " RPM", Grad2))
                .Add(New Monitor_Single("CPU Temp:", message.CPU_Temp, "°C", Grad1))
                .Add(New Monitor_Single("GPU Temp:", message.GPU_Temp, "°C", Grad2))
            End With

            MonitorsCollection = tempMonitor

        End Sub


        Private Function RPMSmoother(val As Integer) As Double
            If val > 5000 OrElse val < -1 Then Return CPURPMSpeed.Values(CPURPMSpeed.Values.Count - 1).value
            Return (val \ 100) * 100
        End Function





    End Class

    Module GraphManager
        Public Grad1 As New LinearGradientBrush() With {.StartPoint = New Point(0, 0.5), .EndPoint = New Point(1, 0.5), .GradientStops = GradientMap("#f43c39", "#ff8611")}
        Public Grad2 As New LinearGradientBrush() With {.StartPoint = New Point(0, 0.5), .EndPoint = New Point(1, 0.5), .GradientStops = GradientMap("#7652ff", "#19e2ba")}


        Private Function HexToRGB(Input As String) As Color
            Input = Input.Replace("#", "")
            Dim split = Enumerable.Range(0, Input.Length \ 2).Select(Function(i) Input.Substring(i * 2, 2))
            Dim r = CInt("&H" & split(0))
            Dim g = CInt("&H" & split(1))
            Dim b = CInt("&H" & split(2))
            Return Color.FromRgb(r, g, b)
        End Function

        Public Function GradientMap(color1 As String, color2 As String) As GradientStopCollection
            Dim cl1 = HexToRGB(color1)
            Dim cl2 = HexToRGB(color2)
            Dim g_Map As New GradientStopCollection
            g_Map.Add(New GradientStop(cl1, 0))
            g_Map.Add(New GradientStop(cl2, 1))
            Return g_Map
        End Function



    End Module

End Namespace

