Imports Stylet

Namespace ViewModels
    Public Class ToolbarViewModel : Inherits Screen
        Dim windowManager As WindowManager
        Sub New(windowManager As IWindowManager)
            Me.windowManager = windowManager

        End Sub

        Sub Close()
            Application.Current.Shutdown()
        End Sub

        Sub TogglePause()
            BaseTimer.Enabled = Not BaseTimer.Enabled
        End Sub

        Sub ShowInfo()
            Dim Info As New InfoViewModel
            windowManager.ShowWindow(Info)

        End Sub

    End Class
End Namespace
