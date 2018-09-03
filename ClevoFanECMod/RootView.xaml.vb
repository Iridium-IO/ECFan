Public Class RootView
    Sub New()
        InitializeComponent()
        Application.Current.MainWindow = Me
    End Sub

#Region "Move and Resize"
    Private _startPosition As Point

    Private _isResizing As Boolean = False

    Dim MinW As Double = 300
    Dim MinH As Double = 450

    Private Sub resizeGrip_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If Mouse.Capture(Me.resizeGrip) Then
            _isResizing = True
            _startPosition = Mouse.GetPosition(Me)
        End If
    End Sub

    Private Sub window_PreviewMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If _isResizing Then
            Dim currentPosition As Point = Mouse.GetPosition(Me)
            Dim diffX As Double = currentPosition.X - _startPosition.X
            Dim diffY As Double = currentPosition.Y - _startPosition.Y
            _startPosition = currentPosition
            If Me.Height >= MinH Then Me.Height += diffY
            If Me.Width >= MinW Then Me.Width += diffX

        End If
    End Sub

    Private Sub resizeGrip_PreviewMouseLeftButtonUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If _isResizing = True Then
            _isResizing = False
            Mouse.Capture(Nothing)
        End If

        If Me.Width <= MinW Then Me.Width = MinW

        If Me.Height <= MinH Then Me.Height = MinH
    End Sub

#End Region


End Class
