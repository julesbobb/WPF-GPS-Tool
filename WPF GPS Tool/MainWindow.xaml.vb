Imports Classes
Imports System.ComponentModel
Imports Classes.Events

Class MainWindow

#Region "Properties"

    Dim WithEvents BackgroundWorker1 As New BackgroundWorker


#End Region

#Region "Modifiers"

#Region "Private"

    Shared _spManager As PortManager
    Dim pUIViewModel As New UIDataViewModel

#End Region

#Region "Friend"

    Friend GPS As GPSLine

#End Region

#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.DataContext = Me.pUIViewModel
        UserInitialization()

    End Sub

    Private Sub UserInitialization()
        _spManager = New PortManager(pUIViewModel)
        With _spManager
            AddHandler .NewSerialDataRecieved, AddressOf _spManager_NewSerialDataRecieved
            AddHandler .WriteOutput, AddressOf WriteOutput
            AddHandler .UpdateLogEvent, AddressOf UpdateLogFile
        End With
    End Sub

#Region "Handler Events"

    Private Sub UpdateLogFile(sender As Object, e As UpdateEventsArgs)
        If e.AddCrLf = True Then
            pUIViewModel.LogText = pUIViewModel.LogText + e.LogText + ControlChars.CrLf
        Else
            pUIViewModel.LogText = pUIViewModel.LogText + e.LogText
        End If

        Try
            tbLog.ScrollToLine(tbLog.LineCount - 1)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ResetPortList()
        Dim mySerialSettings As Settings = _spManager.CurrentSerialSettings
        cmbPorts.ItemsSource = mySerialSettings.portList.ToArray
    End Sub

       ''' <summary>
    ''' Enter the readline in the log text box and, validate the checksum and decode the signal
    ''' </summary>
    ''' <param name="line"></param>
    ''' <remarks></remarks>
    Private Sub ReadGPSFeed(line As String)
        GPS = New GPSLine(_spManager, pUIViewModel)
        With GPS
            AddHandler .IsGPSSignal, AddressOf IsGPS
        End With
        With GPS
            If .IsValidCheckSum(line) Then
                .DecodeSignal(line)
            End If
        End With
    End Sub

#Region "GPSLine Event Handlers"

    ''' <summary>
    ''' Exit point of Application when the ProvideOutput Boolean is set to True
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared Sub WriteOutput(sender As Object, e As WriteOutputEventArgs)
        _spManager.StopListening()
        Environment.Exit(CType(e.Code, OutputCode).GetHashCode)
    End Sub

    Private Sub IsGPS(value As Boolean)
        _spManager.IsGPS = value
        If value = True Then
            pUIViewModel.StatusLabelText = _spManager.CurrentSerialSettings.PortName + " is connected to a valid GPS receiver"
            pUIViewModel.LogText = $"{pUIViewModel.LogText}{pUIViewModel.CurrentPort} is connected to a valid GPS receiver{ControlChars.CrLf}"
        End If
    End Sub

#End Region

    Private Sub _spManager_NewSerialDataRecieved(sender As Object, e As SerialDataEventArgs)
        pUIViewModel.LogText = pUIViewModel.LogText + e.ReadLine
        ReadGPSFeed(e.ReadLine)
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            ReadGPSFeed(e.Argument)
        Catch ex As Exception
            Exit Sub
        End Try
    End Sub

#End Region

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        cmbBaudRate.Text = pUIViewModel.DefaultBaudRate
        If pUIViewModel.PortNameCollection.Count > 0 Then
            cmbPorts.Text = pUIViewModel.PortNameCollection(0).ToString
            _spManager.CollectParameter()
        Else
            'No COM ports so exit the application
            MessageBox.Show("No COM ports detected")
            WriteOutput(Me, New WriteOutputEventArgs With {.Code = OutputCode.NO_GPS_DETECTED})
        End If
    End Sub

#Region "Controls Events"

    Private Sub cmbPorts_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        pUIViewModel.CurrentPort = cmbPorts.SelectedItem.ToString
    End Sub

    Private Sub cmbBaudRate_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        pUIViewModel.BaudRate = CType(cmbBaudRate.SelectedItem.ToString, Integer)
    End Sub

    Private Sub btnAutoScan_Click(sender As Object, e As RoutedEventArgs) Handles btnAutoScan.Click
        _spManager.StartListening(pUIViewModel.BaudRate)
        pUIViewModel.AutoScanEnabled = False
    End Sub

    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs) Handles btnStart.Click
        If pUIViewModel.StartButtonContext = "Connect" Then
            _spManager.StartListening(pUIViewModel.CurrentPort, pUIViewModel.BaudRate)
            progress.Visibility = Visibility.Visible
            lblScan.Visibility = Visibility.Visible
        Else
            _spManager.StopListening()
            pUIViewModel.StartButtonContext = "Connect"
            progress.Visibility = Visibility.Hidden
            lblScan.Visibility = Visibility.Hidden
        End If
    End Sub

    Private Sub tbLog_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbLog.TextChanged
        tbLog.ScrollToLine(tbLog.LineCount - 1)
    End Sub

#End Region

End Class
