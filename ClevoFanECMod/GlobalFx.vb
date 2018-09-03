Imports NvAPIWrapper.Native
Imports Stylet
Module GlobalFx

    Public eventAggregator As IEventAggregator = New EventAggregator

    Public WithEvents BaseTimer As New Timers.Timer With {.Interval = 1000}

    ''' <summary>
    ''' Returns the GP Temperature as a formatted string.
    ''' </summary>
    ''' <param name="Fahrenheit">Optionally returns the temperature in Degrees Fahrenheit</param>
    ''' <returns></returns>
    Function GetGPUTemp(Optional Fahrenheit As Boolean = False) As String

        Dim GPU_THERMALSENSORS = GPUApi.GetThermalSettings(GPUApi.EnumPhysicalGPUs(0), GPU.ThermalSettingsTarget.All)
        Dim GPU1_TX = GPU_THERMALSENSORS.First(Function(x) x.Target.ToString = "GPU")
        Dim Temp = GPU1_TX.CurrentTemperature

        If Fahrenheit Then
            Temp = Temp * 1.8 + 32
            Return Temp
        End If

        Return Temp
    End Function

    Function ReadConfigXML(Section As String, Value As String) As String
        Dim s As New IO.StreamReader("Settings.ini")

        Debug.WriteLine(s.ReadToEnd)

    End Function

    Function VBHex(input As String) As String
        Return input.Replace("0x", "&h")
    End Function


End Module
