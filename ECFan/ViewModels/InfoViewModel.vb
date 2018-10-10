Imports Stylet

Namespace ViewModels
    Public Class InfoViewModel : Inherits Screen

        Property Version As String = String.Format("Version {0}.{1}", My.Application.Info.Version.Major, My.Application.Info.Version.Minor)

        Sub RequestURI(reqURI As String)
            Process.Start(reqURI)
        End Sub

    End Class
End Namespace
