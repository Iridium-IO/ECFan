Imports Stylet
Imports System.Text.RegularExpressions
Imports org.mariuszgromada.math.mxparser
Imports LiveCharts
Imports LiveCharts.Wpf
Imports ClevoFanECMod.Models
Imports LiveCharts.Configurations

Namespace ViewModels

    Public Class MonitorViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)

        Public Property MonitorsSet As New BindableCollection(Of Monitor)

        Public Property SeriesCollection As New BindableCollection(Of Series)

        Public Property MainCollection As New SeriesCollection

        Public Property DateTimeFormatter As Func(Of Double, String) = Function(x) (New DateTime(CLng(x)).ToString("HH:mm:ss"))


        Sub New(ByRef eventAggregator As IEventAggregator)
            eventAggregator.Subscribe(Me)


            Dim mapper = Mappers.Xy(Of MeasureModel)().X(Function(x) x.datetime.Ticks).Y(Function(y) y.value)
            Charting.For(Of MeasureModel)(mapper)

            SeriesCollection.Add(CPUTemp)
            SeriesCollection.Add(GPUTemp)
            SeriesCollection.Add(CPURPMSpeed)
            SeriesCollection.Add(GPURPMSpeed1)
            SeriesCollection.Add(GPURPMSpeed2)
            MainCollection.AddRange(SeriesCollection)

            SetAxisLimits(DateTime.Now)

        End Sub

        Private Property CPUTemp As New LineSeries With {.Title = "CPU Temp", .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 5}
        Private Property GPUTemp As New LineSeries With {.Title = "GPU Temp", .Values = New ChartValues(Of MeasureModel)(), .Fill = Brushes.Transparent, .PointGeometrySize = 5}

        Private Property CPURPMSpeed As New LineSeries With {.Title = "CPU RPM", .Values = New ChartValues(Of MeasureModel)(), .ScalesYAt = 1, .Fill = Brushes.Transparent, .PointGeometrySize = 5}
        Private Property GPURPMSpeed1 As New LineSeries With {.Title = "GPU RPM 1", .Values = New ChartValues(Of MeasureModel)(), .ScalesYAt = 1, .Fill = Brushes.Transparent, .PointGeometrySize = 5}
        Private Property GPURPMSpeed2 As New LineSeries With {.Title = "GPU RPM 2", .Values = New ChartValues(Of MeasureModel)(), .ScalesYAt = 1, .Fill = Brushes.Transparent, .PointGeometrySize = 5}


        Public Property AxisStep As Double = TimeSpan.FromSeconds(22).Ticks
        Public Property AxisUnit As Double = TimeSpan.TicksPerSecond
        Public Property AxisMax As Double
        Public Property AxisMin As Double



        Private Sub SetAxisLimits(now As DateTime)
            AxisMax = now.Ticks
            AxisMin = now.Ticks - TimeSpan.FromSeconds(20).Ticks
        End Sub


        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle
            Dim tempMonitor As New BindableCollection(Of Monitor)

            tempMonitor.Add(New Monitor("CPU", message.CPU_Temp, message.CPU_Fan_RPM1, ""))
            tempMonitor.Add(New Monitor("GPU", message.GPU_Temp, message.GPU_Fan_RPM1, message.GPU_Fan_RPM2))
            MonitorsSet = tempMonitor

            Dim now = DateTime.Now
            CPUTemp.Values.Add(New MeasureModel With {.datetime = now, .value = CInt(message.CPU_Temp)})
            GPUTemp.Values.Add(New MeasureModel With {.datetime = now, .value = CInt(message.GPU_Temp)})
            CPURPMSpeed.Values.Add(New MeasureModel With {.datetime = now, .value = RPMSmoother(message.CPU_Fan_RPM1)})
            GPURPMSpeed1.Values.Add(New MeasureModel With {.datetime = now, .value = RPMSmoother(message.GPU_Fan_RPM1)})
            GPURPMSpeed2.Values.Add(New MeasureModel With {.datetime = now, .value = RPMSmoother(message.GPU_Fan_RPM2)})

            If CPUTemp.Values.Count > 23 Then
                For Each i In SeriesCollection
                    i.Values.RemoveAt(0)
                Next
            End If

            SetAxisLimits(now)


        End Sub

        Private Function RPMSmoother(val As Integer) As Double
            If val > 5000 OrElse val < -1 Then
                Return CPURPMSpeed.Values(CPURPMSpeed.Values.Count - 1).value
            Else
                Return (val \ 200) * 200
            End If


        End Function


    End Class




End Namespace

