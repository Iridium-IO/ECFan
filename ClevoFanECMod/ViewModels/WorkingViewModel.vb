Imports Stylet

Namespace ViewModels
    Public Class WorkingViewModel : Inherits Screen : Implements IHandle(Of SensorEvent)

        Property Current_Idle As String = "200 RPM"
        Property Current_Max As String = "3400 RPM"

        Property New_Idle As String = "20 RPM"
        Property New_Max As String = "40 RPM"


        Sub New()
            eventAggregator.Subscribe(Me)
        End Sub

        Public Sub Handle(message As SensorEvent) Implements IHandle(Of SensorEvent).Handle



        End Sub
    End Class
End Namespace
