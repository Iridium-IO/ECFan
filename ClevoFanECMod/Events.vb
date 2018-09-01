Imports System.Text.RegularExpressions
Imports org.mariuszgromada.math.mxparser

Public Class SensorEvent

    Private ReferenceXElement As XElement
    Dim XElementOffsets As XElement
    Dim XElementFunctions As XElement

    Sub New(ByRef LaptopSettingsXElement As XElement)
        ReferenceXElement = LaptopSettingsXElement
        XElementOffsets = ReferenceXElement.<Offsets>.Single
        XElementFunctions = ReferenceXElement.<Functions>.Single
    End Sub

    Sub Populate(ByRef dict As Dictionary(Of String, Integer))
        For Each Offset In XElementOffsets.Elements
            MapDictionaryToConfig(Offset.Name.LocalName, dict)
        Next
    End Sub

    Sub MapDictionaryToConfig(targetConfig As String, ByRef dict As Dictionary(Of String, Integer))

        Dim WorkingConfigVal = XElementOffsets.Element(targetConfig).Value
        If dict.Count > 0 Then
            If WorkingConfigVal.Contains(",") Then
                Dim offset1 = WorkingConfigVal.Split(",")(0)
                Dim offset2 = WorkingConfigVal.Split(",")(1)
                Dim dictVal1 = dict.Keys.Where(Function(x) x.Equals(offset1)).Single
                Dim dictVal2 = dict.Keys.Where(Function(x) x.Equals(offset2)).Single

                CallByName(Me, targetConfig, CallType.Set,
                           String.Concat("&H", Hex(dict(dictVal1)), Hex(dict(dictVal2))))
            Else
                Dim dictionaryVal = dict.Keys.Where(Function(x) WorkingConfigVal.Equals(x)).Single
                CallByName(Me, targetConfig, CallType.Set, dict(dictionaryVal))

            End If
        End If


    End Sub

    Public Property GPU_Temp = 0
    Public Property CPU_Temp = 0
    Public Property CPU_Fan_Duty = 0
    Public Property GPU_Fan_Duty = 0
    Public Property GPU_Fan_Target = 0
    Private _CPU_Fan_RPM1 As Integer = -1
    Public Property CPU_Fan_RPM1
        Get
            Return _CPU_Fan_RPM1
        End Get
        Set(value)
            Dim xm = New Expression(Regex.Replace(XElementFunctions.<FanRPM>.Value, "\[n\]", Val(value).ToString))
            _CPU_Fan_RPM1 = xm.calculate
        End Set
    End Property
    Private _GPU_Fan_RPM1 As Integer = -1
    Public Property GPU_Fan_RPM1
        Get
            Return _GPU_Fan_RPM1
        End Get
        Set(value)
            Dim xm = New Expression(Regex.Replace(XElementFunctions.<FanRPM>.Value, "\[n\]", Val(value).ToString))
            _GPU_Fan_RPM1 = xm.calculate
        End Set
    End Property
    Private _GPU_Fan_RPM2 As Integer = -1
    Public Property GPU_Fan_RPM2
        Get
            Return _GPU_Fan_RPM2
        End Get
        Set(value)
            Dim xm = New Expression(Regex.Replace(XElementFunctions.<FanRPM>.Value, "\[n\]", Val(value).ToString))
            _GPU_Fan_RPM2 = xm.calculate
        End Set
    End Property

End Class
