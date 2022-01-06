Imports System.ComponentModel
Imports System.IO.Ports


Namespace Classes

    Public Class Settings

        Private _baudRates As Integer() = New Integer() {2400, 4800, 9600, 19200, 38400, 57600, 115200}
        Dim _portName As String = ""
        Dim _portNameCollection As String()
        Dim _baudRateCollection As New BindingList(Of Integer)()
        Dim _BaudRate As Integer = 4800
        Const _stopBits As StopBits = StopBits.One
        Const _parity As Parity = Parity.None
        Const _dataBits As Integer = 8

#Region "Properties"

        Public Property portList As New List(Of String)

        ''' <summary>
        ''' Gets or sets the Baud Rate
        ''' </summary>
        Public Property BaudRate() As Integer
            Get
                Return _BaudRate
            End Get
            Set(value As Integer)
                _BaudRate = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the default baud rate collection
        ''' </summary>
        Public ReadOnly Property BaudRates As Integer()
            Get
                Return _baudRates
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the available ports on the computer
        ''' </summary>
        Public Property PortNameCollection() As String()
            Get
                Return _portNameCollection
            End Get
            Set(value As String())
                _portNameCollection = value
            End Set
        End Property

        ''' <summary>
        ''' THe name of the port to use for the GPS
        ''' </summary>
        Public Property PortName() As String
            Get
                Return _portName
            End Get
            Set(value As String)
                If Not _portName.Equals(value) Then
                    _portName = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the default Parity value.
        ''' </summary>
        Public ReadOnly Property Parity() As Parity
            Get
                Return _parity
            End Get
        End Property

        ''' <summary>
        ''' The default data bits value.
        ''' </summary>
        Public ReadOnly Property DataBits() As Integer
            Get
                Return _dataBits
            End Get
        End Property

        ''' <summary>
        ''' Get the default StopBits value.
        ''' </summary>
        Public ReadOnly Property StopBits() As StopBits
            Get
                Return _stopBits
            End Get
        End Property

#End Region

        ''' <summary>
        ''' Updates the range of possible baud rates for device
        ''' </summary>
        ''' <param name="possibleBaudRates">dwSettableBaud parameter from the COMMPROP Structure</param>
        Public Sub UpdateBaudRateCollection(possibleBaudRates As Integer)
            Const BAUD_075 As Integer = &H1
            Const BAUD_110 As Integer = &H2
            Const BAUD_150 As Integer = &H8
            Const BAUD_300 As Integer = &H10
            Const BAUD_600 As Integer = &H20
            Const BAUD_1200 As Integer = &H40
            Const BAUD_1800 As Integer = &H80
            Const BAUD_2400 As Integer = &H100
            Const BAUD_4800 As Integer = &H200
            Const BAUD_7200 As Integer = &H400
            Const BAUD_9600 As Integer = &H800
            Const BAUD_14400 As Integer = &H1000
            Const BAUD_19200 As Integer = &H2000
            Const BAUD_38400 As Integer = &H4000
            Const BAUD_56K As Integer = &H8000
            Const BAUD_57600 As Integer = &H40000
            Const BAUD_115200 As Integer = &H20000
            Const BAUD_128K As Integer = &H10000

            _baudRateCollection.Clear()

            If (possibleBaudRates And BAUD_075) > 0 Then
                _baudRateCollection.Add(75)
            End If
            If (possibleBaudRates And BAUD_110) > 0 Then
                _baudRateCollection.Add(110)
            End If
            If (possibleBaudRates And BAUD_150) > 0 Then
                _baudRateCollection.Add(150)
            End If
            If (possibleBaudRates And BAUD_300) > 0 Then
                _baudRateCollection.Add(300)
            End If
            If (possibleBaudRates And BAUD_600) > 0 Then
                _baudRateCollection.Add(600)
            End If
            If (possibleBaudRates And BAUD_1200) > 0 Then
                _baudRateCollection.Add(1200)
            End If
            If (possibleBaudRates And BAUD_1800) > 0 Then
                _baudRateCollection.Add(1800)
            End If
            If (possibleBaudRates And BAUD_2400) > 0 Then
                _baudRateCollection.Add(2400)
            End If
            If (possibleBaudRates And BAUD_4800) > 0 Then
                _baudRateCollection.Add(4800)
            End If
            If (possibleBaudRates And BAUD_7200) > 0 Then
                _baudRateCollection.Add(7200)
            End If
            If (possibleBaudRates And BAUD_9600) > 0 Then
                _baudRateCollection.Add(9600)
            End If
            If (possibleBaudRates And BAUD_14400) > 0 Then
                _baudRateCollection.Add(14400)
            End If
            If (possibleBaudRates And BAUD_19200) > 0 Then
                _baudRateCollection.Add(19200)
            End If
            If (possibleBaudRates And BAUD_38400) > 0 Then
                _baudRateCollection.Add(38400)
            End If
            If (possibleBaudRates And BAUD_56K) > 0 Then
                _baudRateCollection.Add(56000)
            End If
            If (possibleBaudRates And BAUD_57600) > 0 Then
                _baudRateCollection.Add(57600)
            End If
            If (possibleBaudRates And BAUD_115200) > 0 Then
                _baudRateCollection.Add(115200)
            End If
            If (possibleBaudRates And BAUD_128K) > 0 Then
                _baudRateCollection.Add(128000)
            End If

        End Sub

    End Class

End Namespace
