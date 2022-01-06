Imports System.IO.Ports
Imports System.Reflection
Imports Classes.Events
Imports System.Timers
Imports System.ComponentModel

Namespace Classes

    ''' <summary>
    ''' Manager for serial port data
    ''' </summary>
    ''' <remarks>When a port is opened, it may open without problems but not immediately pass any information in to the 
    ''' data received event handler or the data may be incorrect. Each connection will be given 3 seconds to establish a clean signal. If it can't be 
    ''' established by then, whether the port is a valid GPS or not, then it will be rejected.</remarks>
    Public Class PortManager
        Implements IDisposable

        Friend Property ProvideOutput As Boolean = True
        Friend Property CheckToPerform As PerformCheck

        Dim pUIViewModel As UIDataViewModel

        Public Sub New(pUIViewModel As UIDataViewModel)
            ' Finding installed serial ports on hardware
            _currentSerialSettings.PortNameCollection = SerialPort.GetPortNames()
            _currentSerialSettings.portList = SerialPort.GetPortNames.ToList
            CheckForValidCOM()

            ' If serial ports is found, we select the first found
            If _currentSerialSettings.PortNameCollection.Length > 0 Then
                _currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection(0)
            End If

            Me.pUIViewModel = pUIViewModel

        End Sub

        Public Sub New()
            ' Finding installed serial ports on hardware
            _currentSerialSettings.PortNameCollection = SerialPort.GetPortNames()
            _currentSerialSettings.portList = SerialPort.GetPortNames.ToList
            CheckForValidCOM()

            ' If serial ports is found, we select the first found
            If _currentSerialSettings.PortNameCollection.Length > 0 Then
                _currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection(0)
            End If
        End Sub

#Region "Fields"

        Private _serialPort As SerialPort
        Private bProvideOutput As Boolean = True
        Public _currentSerialSettings As New Settings()
        Private _isRecording As Boolean = False
        Private _RecordedText As String
        Private _TextFilePath As String
        Private _IsGPS As Boolean = False
        Private _HasIsGPSResult As Boolean = False

        Private WithEvents _timeOutTimer As New Timer With {.Interval = 5000}   '5 Seconds
        Private _connectionTpye As ConnectionType
        Friend Const NO_GPS_IS_CONNECTED As String = "No GPS is connected"

#End Region

#Region "Events"

        Public Event NewSerialDataRecieved As EventHandler(Of SerialDataEventArgs)
        Public Event RecordingState(ByVal visible As Boolean)
        Public Event UpdateLogEvent As EventHandler(Of UpdateEventsArgs)
        Public Event WriteOutput As EventHandler(Of WriteOutputEventArgs)

        ''' <summary>
        ''' Rebuilds port list combo box
        ''' </summary>
        ''' <remarks></remarks>
        Public Event ResetPortList()

#End Region

#Region "Properties"

        WriteOnly Property TextFilePath As String
            Set(value As String)
                _TextFilePath = value
            End Set
        End Property

        WriteOnly Property IsGPS As Boolean
            Set(value As Boolean)
                _IsGPS = value
                _HasIsGPSResult = True
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the current serial port settings
        ''' </summary>
        Public Property CurrentSerialSettings() As Settings
            Get
                If IsNothing(_currentSerialSettings) Then
                    _currentSerialSettings = New Settings
                End If
                Return _currentSerialSettings
            End Get
            Set(value As Settings)
                _currentSerialSettings = value
            End Set
        End Property

        Public ReadOnly Property IsRecording As Boolean
            Get
                Return _isRecording
            End Get
        End Property

#End Region

#Region "Event Handlers"

        Private Sub _currentSerialSettings_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
            ' if serial port is changed, a new baud query is issued
            If e.PropertyName.Equals("PortName") Then
                ' UpdateBaudRateCollection()
            End If
        End Sub


        Private Sub _serialPort_DataReceived(sender As Object, e As SerialDataReceivedEventArgs)
            Try

                If _serialPort.IsOpen Then

                    Dim data As String = _serialPort.ReadLine

                    If data.Length = 0 Or String.IsNullOrEmpty(data) Then
                        Return
                    End If

                    ' Send data to whom ever interested
                    RaiseEvent NewSerialDataRecieved(Me, New SerialDataEventArgs(data))
                    If _isRecording Then
                        _RecordedText += data + ControlChars.CrLf
                    End If
                End If
            Catch ex As Exception
                Return
            End Try

        End Sub


#End Region

#Region "Methods"

        ''' <summary>
        ''' Manually connects to a serial port defined through the current settings
        ''' </summary>
        ''' <param name="portName">The port name to use</param>
        Public Sub StartListening(portName As String, ByVal baudRate As Integer)
            ' Closing serial port if it is open
            _HasIsGPSResult = False
            If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then
                _serialPort.Close()
            End If

            _currentSerialSettings.BaudRate = baudRate

            ConnectManually(portName)

        End Sub

        ''' <summary>
        ''' Auto connects to a serial port defined through the current settings
        ''' </summary>
        Public Sub StartListening(ByVal baudRate As Integer)
            ' Closing serial port if it is open
            _HasIsGPSResult = False
            If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then
                _serialPort.Close()
            End If

            _currentSerialSettings.BaudRate = baudRate

            AutoConnect()

        End Sub

        Public Sub ForceStaticTest()
            RemoveHandler _serialPort.DataReceived, AddressOf _serialPort_DataReceived
            System.Threading.Thread.Sleep(500)
            With _serialPort
                .WriteLine(ControlChars.CrLf + "$LTC,1552,1*69")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$LTC,1551,1,4800,2,1,4,1,5,3,6,1,7,1,21,1*79")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,00,00,01,01*25")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,02,00,01,01*27")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,03,00,03,01*24")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,04,00,01,01*21")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,05,00,01,01*20")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$LTC,1114,1,1*72")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF100,1,4800,8,1,0*0E")
            End With
        End Sub

        Public Sub DisableForceStaticTest()
            RemoveHandler _serialPort.DataReceived, AddressOf _serialPort_DataReceived
            System.Threading.Thread.Sleep(500)
            With _serialPort
                .WriteLine(ControlChars.CrLf + "$LTC,1552,1*69")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$LTC,1551,1,4800,2,1,4,1,5,3,6,1,7,1,21,0*78")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,00,00,01,01*25")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,02,00,01,01*27")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,03,00,03,01*24")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,04,00,01,01*21")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF103,05,00,01,01*20")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$LTC,1114,1,0*73")
                System.Threading.Thread.Sleep(500)
                .WriteLine(ControlChars.CrLf + "$PSRF100,1,4800,8,1,0*0E")
            End With
        End Sub

        ''' <summary>
        ''' Get the input parameters, which identifies the test to perform
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub CollectParameter()
            Const ERROR_BAD_ARGUMENTS As Integer = &HA0
            Const ERROR_INVALID_COMMAND_LINE As Integer = &H667

            Dim args() As String = Environment.GetCommandLineArgs()

            Const arrayLength As Integer = 2
            Const iTryParse As Integer = 1

            If args.Length = arrayLength Then
                Dim value As Integer = 0
                If Integer.TryParse(args(iTryParse), value) Then
                    If value >= 1 And value <= 5 Then
                        'Valid parameter has been passed in
                        CheckToPerform = value
                    Else
                        Environment.Exit(ERROR_BAD_ARGUMENTS)
                    End If
                Else
                    Environment.Exit(ERROR_INVALID_COMMAND_LINE)
                End If
            Else
                ProvideOutput = False
            End If
        End Sub

        ''' <summary>
        ''' Performs the force static test on the open serial connection
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub PerformForceStatic()
            Dim output As OutputCode
            Try
                ForceStaticTest()
                output = OutputCode.GPS_DETECTED_STATIC_FORCED_ON
            Catch ex As Exception
                output = OutputCode.NO_GPS_DETECTED
            End Try
            RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = output})
        End Sub

        ''' <summary>
        ''' Enables the WriteOutput event to be called outside of the class
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub RaiseWriteOutput(code As OutputCode)
            RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = code})
        End Sub

        ''' <summary>
        ''' Performs the disable force static test on the open serial connection
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub PerformDisableForceStatic()
            Dim output As OutputCode
            Try
                DisableForceStaticTest()
                output = OutputCode.GPS_DETECTED_STATIC_FORCED_OFF
            Catch ex As Exception
                output = OutputCode.NO_GPS_DETECTED
            End Try
            RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = output})
        End Sub

        ''' <summary>
        ''' Timeout timer for AutoScanCOMPorts
        ''' </summary>
        ''' <remarks></remarks>
        Dim WithEvents timeout As New Timer With {.Interval = 3000} '3 Seconds
        ''' <summary>
        ''' Indicates that the timeout has passed
        ''' </summary>
        ''' <remarks></remarks>
        Dim timeoutInitiated As Boolean = False

        ''' <summary>
        ''' Port in the _currentSerialSettings.portList being scanned during the ScanPort process
        ''' </summary>
        ''' <remarks></remarks>
        Dim _port As String

        ''' <summary>
        ''' Indicate the time out and scan the new port during the AutoScanCOMPorts loop
        ''' </summary>
        ''' <remarks></remarks>
        Dim _ScanNextPort As Boolean

        Private Sub timeout_Tick(sender As Object, e As EventArgs) Handles timeout.Elapsed
            timeoutInitiated = True
            _ScanNextPort = True
        End Sub

#Region "Auto Scan"

        ''' <summary>
        ''' Attempt to connect to each COM port and determine if they are GPS enabled
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub AutoConnect()

            Me.BackgroundWorker1.RunWorkerAsync()

        End Sub

        Private WithEvents cTestGPS As AutoScan
        Private WithEvents GPSCOM As AutoScan

        Dim WithEvents BackgroundWorker1 As New BackgroundWorker

        Private GPSComPort As SerialPort
        Private IsGPSPort As Boolean
        Dim portdetected As Boolean = False

        Private _baudRatesToAutoScan As Integer() = New Integer() {4800, 9600}

        Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
            For Each port In _currentSerialSettings.portList
                For Each baud As Int16 In _baudRatesToAutoScan
                    cTestGPS = New AutoScan(port, baud)
                    RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = String.Format("Checking {0} (baud {1})...", port, baud.ToString), .AddCrLf = False})
                    With cTestGPS
                        .DoWait()
                        If .IsGPS Then
                            portdetected = True
                            GPSComPort = .GPSPort
                            GPSCOM = cTestGPS
                            IsGPSPort = True
                            RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = "...GPS detected"})
                            ' Subscribe to event and open serial port for data
                            _currentSerialSettings.BaudRate = GPSComPort.BaudRate
                            ConnectManually(GPSComPort.PortName)
                            Exit For
                        Else
                            RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = "...not a GPS receiver"})
                        End If
                    End With
                Next
                Try
                Catch ex As ApplicationException
                    RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = ex.Message})
                End Try
            Next
        End Sub

        Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

            If portdetected = False Then
                RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = "Cannot find a GPS device"})
                pUIViewModel.UpdateStatusLabelHander(New UpdateEventsArgs With {.LogText = "No GPS is connected"})
                CheckInputOutputParameters(OutputCode.NO_GPS_DETECTED)
                pUIViewModel.grpBoxSettingsIsEnabled = True
            End If
        End Sub

#End Region

#Region "Manage Parameter"

        Private Sub CheckInputOutputParameters(output As OutputCode)

            Select Case output = OutputCode.NO_GPS_DETECTED
                Case OutputCode.NO_GPS_DETECTED
                    If MessageBox.Show("The current COM is not recognized as a GPS. Do you wish to try again?", "Unrecognized COM", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
                        Exit Sub
                    Else
                        RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = output})
                    End If
                Case OutputCode.FAIL_USER_ENDED_APP_MANUALLY
                    RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = output})
                Case Else
                    If bProvideOutput = True Then
                        Select Case CheckToPerform
                            Case PerformCheck.DETECT_GPS_ONLY
                                RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = output})
                        End Select

                    End If
            End Select
        End Sub

#End Region

#Region "ConnectManually"

        ''' <summary>
        ''' Sets the selected COM port and calls the procedure to connect manaully
        ''' </summary>
        ''' <param name="portname"></param>
        ''' <remarks></remarks>
        Private Sub ConnectManually(portname As String)
            _currentSerialSettings.PortName = portname
            pConnectManually(_currentSerialSettings.PortName)
        End Sub

        ''' <summary>
        ''' Start the OpenConnectionTimer based on the TimeoutPeriodMinutes setting. The timer object is kept alive which prevents the Garbage Collector from sweeping it up
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub StartConnectionTimer()
            OpenConnectionTimer = New Timer(10000 * 6 * My.Settings.TimeoutPeriodMinutes)
            GC.KeepAlive(OpenConnectionTimer) 'as this is a long Timer event, this prevents the Garbage Collector from sweeping it up
            OpenConnectionTimer.Start()
        End Sub

        Private WithEvents OpenConnectionTimer As Timer

        Private Sub pConnectManually(portName As String)
            ' Setting serial port settings
            With _currentSerialSettings
                _serialPort = New SerialPort(portName, .BaudRate, .Parity, .DataBits, .StopBits)
            End With
            RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = String.Format("Connecting to {0} (baud {1})...", _serialPort.PortName, _serialPort.BaudRate.ToString), .AddCrLf = False})
            ' Subscribe to event and open serial port for data
            AddHandler _serialPort.DataReceived, AddressOf _serialPort_DataReceived
            Try
                _serialPort.Open()
                If _serialPort.IsOpen Then
                    RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = "Open"})
                    StartConnectionTimer()
                Else
                    RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = "Cannot open"})
                End If
            Catch ex As Exception
                RemoveHandler _serialPort.DataReceived, AddressOf _serialPort_DataReceived
            End Try
            ConnectMessage()
            _timeOutTimer.Start()
        End Sub

#End Region

        Private Sub ConnectMessage()
            If IsNothing(_serialPort) Then
                Exit Sub
            End If
            With _serialPort
                If .IsOpen Then
                    pUIViewModel.UpdateStatusLabelHander(New UpdateEventsArgs With {.LogText = _serialPort.PortName + " is open"})
                    pUIViewModel.UpdateConnectButtons(New ConnectionStateEventsArgs(SerialConnectionState.Open))

                    '    RaiseEvent UpdateConnectButtons(Me, New ConnectionStateEventsArgs(SerialConnectionState.Open))
                Else
                    pUIViewModel.UpdateStatusLabelHander(New UpdateEventsArgs With {.LogText = NO_GPS_IS_CONNECTED})
                    pUIViewModel.UpdateConnectButtons(New ConnectionStateEventsArgs(SerialConnectionState.Closed))
                    '     RaiseEvent UpdateConnectButtons(Me, New ConnectionStateEventsArgs(SerialConnectionState.Closed))
                End If
            End With
        End Sub

        ''' <summary>
        ''' Closes the serial port
        ''' </summary>
        Public Sub StopListening()
            If Not IsNothing(_serialPort) Then
                If _serialPort.IsOpen Then
                    _serialPort.Close()
                    ConnectMessage()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Retrieves the current selected device's COMMPROP structure, and extracts the dwSettableBaud property
        ''' </summary>
        Private Sub UpdateBaudRateCollection()
            Try
                _serialPort = New SerialPort(_currentSerialSettings.PortName)
                _serialPort.Open()
                Dim p As Object = _serialPort.BaseStream.[GetType]().GetField("commProp", BindingFlags.Instance Or BindingFlags.NonPublic).GetValue(_serialPort.BaseStream)
                Dim dwSettableBaud As Int32 = DirectCast(p.[GetType]().GetField("dwSettableBaud", BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.[Public]).GetValue(p), Int32)

                _serialPort.Close()
                _currentSerialSettings.UpdateBaudRateCollection(dwSettableBaud)
            Catch ex As Exception
                'remove port from list
                _currentSerialSettings.portList.Remove(_currentSerialSettings.PortName)
                RaiseEvent ResetPortList()
                Exit Sub
            End Try
        End Sub

        ''' <summary>
        ''' Confirm that the COM has both access. If not then remove them from the _currentSerialSettings.portList
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub CheckForValidCOM()
            'SerialPort.GetPortNames
            For Each port As String In SerialPort.GetPortNames
                Dim testPort As New SerialPort(port)
                Try
                    testPort.Open()
                    'Opened successfully
                    testPort.Close()
                Catch ex As Exception
                    _currentSerialSettings.portList.Remove(port)
                End Try
            Next
        End Sub


        ' Part of basic design pattern for implementing Dispose
        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposing Then
                If _serialPort IsNot Nothing Then
                    If _serialPort.IsOpen Then
                        RemoveHandler _serialPort.DataReceived, AddressOf _serialPort_DataReceived
                    End If
                End If
            End If
            ' Releasing serial port (and other unmanaged objects)
            If _serialPort IsNot Nothing Then
                If _serialPort.IsOpen Then
                    _serialPort.Close()
                End If
                _serialPort.Dispose()
            End If
        End Sub

#End Region

        ''' <summary>
        ''' Call to release serial port
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

        Private Sub _timeOutTimer_Tick(sender As Object, e As EventArgs) Handles _timeOutTimer.Elapsed
            'Check if port is open
            If _serialPort.IsOpen Then
                If Not _IsGPS Then
                    _serialPort.Dispose()
                    If _connectionTpye = Enums.ConnectionType.Manual Then
                        NotRecognisedGPS(_serialPort)
                    ElseIf _connectionTpye = ConnectionType.AutoScan Then
                        'Scan next COM port in the list
                        _ScanNextPort = True
                    End If
                    CheckInputOutputParameters(OutputCode.NO_GPS_DETECTED)
                Else
                    _timeOutTimer.Stop()
                    OpenConnectionTimer.Stop()
                    Select Case CheckToPerform
                        Case PerformCheck.DETECT_GPS_ONLY
                            CheckInputOutputParameters(OutputCode.GPS_DETECTED)
                    End Select
                End If
            End If
        End Sub

        Private Sub NotRecognisedGPS(serial As SerialPort)

            pUIViewModel.UpdateStatusLabelHander(New UpdateEventsArgs With {.LogText = "Not Connected"})
            RaiseEvent UpdateLogEvent(Me, New UpdateEventsArgs With {.LogText = serial.PortName + " is not a recognized GPS receiver"})
            pUIViewModel.UpdateConnectButtons(New ConnectionStateEventsArgs(SerialConnectionState.Closed))

        End Sub

        Private Sub OpenConnectionTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles OpenConnectionTimer.Elapsed
            'The total connection timer has expired
            'Close the application with the failed output code
            RaiseEvent WriteOutput(Me, New WriteOutputEventArgs With {.Code = OutputCode.NO_GPS_DETECTED})
        End Sub
    End Class

End Namespace

