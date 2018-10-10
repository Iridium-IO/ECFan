Imports System.Runtime.CompilerServices
Imports NvAPIWrapper.Native
Imports Stylet
Module GlobalFx

    Public eventAggregator As IEventAggregator = New EventAggregator

    Public Property BaseTimer As New Timers.Timer With {.Interval = 1000}
    Public IsMonitoringActive As Boolean = False


    Public SettingsXML As XDocument

    Public RW As String


    Function GetGPUTemp() As String

        Dim GPU_THERMALSENSORS = GPUApi.GetThermalSettings(GPUApi.EnumPhysicalGPUs(0), GPU.ThermalSettingsTarget.All)
        Dim GPU1_TX = GPU_THERMALSENSORS.First(Function(x) x.Target.ToString = "GPU")

        Return GPU1_TX.CurrentTemperature

    End Function


    Function VBHex(input As String) As String
        Return input.Replace("0x", "&h")
    End Function

    <Extension()>
    Public Function Clamp(val As Integer, min As Integer, max As Integer) As Integer

        Return If((val < min), min, If((val > max), max, val))

    End Function

End Module
