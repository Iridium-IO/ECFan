Imports Stylet
Imports System.Text.RegularExpressions
Imports org.mariuszgromada.math.mxparser
Imports LiveCharts
Imports LiveCharts.Wpf

Namespace ViewModels

    Public Class MonitorViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)
        Public Property MonitorsSet As New BindableCollection(Of Monitor)

        Public Property MainCollection As New SeriesCollection

        Public Property TimeSp = TimeSpan.TicksPerSecond

        Dim CPUTemp As New LineSeries With {
            .Title = "CPU Temp",
            .Values = New ChartValues(Of Integer) From {0}
        }





        Sub New(ByRef eventAggregator As IEventAggregator)
            eventAggregator.Subscribe(Me)
            MainCollection.Add(CPUTemp)

        End Sub






        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle
            Dim tempMonitor As New BindableCollection(Of Monitor)

            With tempMonitor
                .Add(New Monitor("CPU", message.CPU_Temp, message.CPU_Fan_RPM1, ""))
                .Add(New Monitor("GPU", message.GPU_Temp, message.GPU_Fan_RPM1, message.GPU_Fan_RPM2))

            End With
            CPUTemp.Values.Add(CInt(message.CPU_Temp))
            If CPUTemp.Values.Count > 11 Then
                CPUTemp.Values.RemoveAt(0)
            End If
            MonitorsSet = tempMonitor

        End Sub



    End Class




End Namespace

