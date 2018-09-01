Imports System.ComponentModel

Namespace Models


    Public Class Monitor
        Property Device As String
        Property Temperature As String
        Property FanRPM1 As String
        Property FanRPM2 As String
        Sub New(d As String, t As String, frpm1 As String, frpm2 As String)
            Device = d
            Temperature = If(t <> "", t & "°C", "")
            FanRPM1 = If(frpm1 <> "", frpm1 & " RPM", "")
            FanRPM2 = If(frpm2 <> "", frpm2 & " RPM", "")
        End Sub










    End Class


    Public Class MeasureModel
        Property datetime As DateTime

        Property value As Double
    End Class

End Namespace

