Imports Stylet

Namespace ViewModels
    Public Class ToolbarViewModel : Inherits Screen

        Sub Close()
            Application.Current.Shutdown()
        End Sub

        Sub TogglePause()
            BaseTimer.Enabled = Not BaseTimer.Enabled
        End Sub

    End Class
End Namespace
