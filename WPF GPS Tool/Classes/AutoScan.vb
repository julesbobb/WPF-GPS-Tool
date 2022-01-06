Imports System.IO.Ports

''' <summary>
''' Adds a thread to wait for the port to be read. It is limited to 5 attempts. Once read it then identifies if it is a GPS signal
''' </summary>
''' <remarks></remarks>
Public Class AutoScan

    Friend Property AttemptCount As Int16 = 1
    Private WithEvents _SerialPort As SerialPort
    Private sReadLines As String = ""
    Public Event COMMessage(message As String)

    ''' <summary>
    ''' Serial Port connector for Production use only
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend ReadOnly Property GPSPort As SerialPort
        Get
            Return _SerialPort
        End Get
    End Property

    Friend ReadOnly Property BaudRate As String
        Get
            Return _SerialPort.BaudRate.ToString
        End Get
    End Property

    Private _Message As String
    Friend ReadOnly Property Message As String
        Get
            Return _Message
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(portname As String, baudRate As Integer)
        _SerialPort = New SerialPort
        With _SerialPort
            .PortName = portname
            .BaudRate = baudRate
            AddHandler .DataReceived, AddressOf _serialPortDataReceiver
            _CheckCount = 0
            .Open()
        End With
    End Sub

    Public Sub New(portname As String)
        _SerialPort = New SerialPort
        With _SerialPort
            .PortName = portname
            .BaudRate = 4800
            AddHandler .DataReceived, AddressOf _serialPortDataReceiver
            _CheckCount = 0
        End With
    End Sub

    Friend Sub Open()
        Try
            _SerialPort.Open()
        Catch secEx As UnauthorizedAccessException
            Throw New ApplicationException("Access denied")
        Catch ex As Exception
            Throw New ApplicationException(ex.Message)
        End Try
    End Sub

    Friend Property IsGPS As Boolean

    Private _CheckInProgress As Boolean
    ''' <summary>
    ''' The maximum number of times a line is read before the GPS result is determined 
    ''' </summary>
    ''' <remarks></remarks>
    Private _CheckCount As Integer

    Private Sub _serialPortDataReceiver(sender As Object, e As SerialDataReceivedEventArgs)
        Try
            sReadLines = _SerialPort.ReadLine
        Catch ex As Exception
            Exit Sub
        End Try
    End Sub

    Protected Function ReadLines() As String
        Try
            If sReadLines.Length > 0 Then
                Me.IsGPS = Me.IsGPSFormat(sReadLines)
                Return sReadLines
            Else
                AttemptCount += 1
                Return ""
            End If
        Catch ex As NullReferenceException
            AttemptCount += 1
            Return ""
        End Try
    End Function

    Private Sub ThreadWait()
        Do
            'Start waiting..
            If AttemptCount > 10 Then
                Exit Do
            End If
            System.Threading.Thread.Sleep(500)
        Loop While ReadLines.Length = 0 Or _CheckInProgress = True
    End Sub

    Public Sub DoWait()
        DoWait(TimeSpan.MinValue)
    End Sub

    Private Sub DoWait(timeout As TimeSpan)
        If _SerialPort.IsOpen = False Then
            Me.IsGPS = False
            Exit Sub
        End If
        Dim t As New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf ThreadWait))
        t.Start()
        If timeout > TimeSpan.MinValue Then
            t.Join(timeout)
        Else
            t.Join()
        End If
        RemoveHandler _SerialPort.DataReceived, AddressOf _serialPortDataReceiver
        System.Threading.Thread.Sleep(500)
        If _SerialPort.IsOpen Then _SerialPort.Close()
    End Sub

    ''' <summary>
    ''' Reads the text line to determine if it matches the criteria for a GPS signal
    ''' </summary>
    ''' <param name="line">The ReadLine from the data received event</param>
    Private Function IsGPSFormat(line As String) As Boolean
        'Discard any sentences that do not begin with a $
        '3 attempts to analyze individual lines. That should be enough to discount
        'the possibility of non-dollar lines and to have at least one valid line. If one hasn't been found by then
        'then it has failed
        Dim result As Boolean = False

        If line.Substring(0) <> "$" Then
            If line.Substring(1, 2) = "GP" Then
                _CheckInProgress = False
                result = True
                AttemptCount = 10
            End If
        Else
            _CheckInProgress = True
            _CheckCount += 1
            If _CheckCount = 3 Then
                _CheckInProgress = False
            End If
        End If

        Return result

    End Function

End Class
