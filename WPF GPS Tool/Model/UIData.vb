Imports System.IO.Ports
Imports Classes

Public Class UIData
   
    Dim _spManager As New PortManager
    Dim mySerialSettings As Settings

    Public Sub New()
        mySerialSettings = _spManager.CurrentSerialSettings
    End Sub



    ''' <summary>
    ''' Gets or sets whether the group box displaying the port settings is enabled
    ''' </summary>
    Public Property grpBoxSettingsIsEnabled As Boolean = True

    ''' <summary>
    ''' Gets or sets the number of satellites with a fix
    ''' </summary>
    Public Property SatsWithFix As Integer

    ''' <summary>
    ''' Gets or sets the number of satellites that have reached the minimum signal strength
    ''' </summary>
    Public Property SatsWithRequiredStrength As Integer

    ''' <summary>
    ''' Gets or sets the text displayed on the manual start button 
    ''' </summary>
    Public Property StartButtonContent As String

    ''' <summary>
    ''' Gets or sets the IsEnabled status of the AutoScan button
    ''' </summary>
    Public Property AutoScanEnabled As Boolean

    ''' <summary>
    ''' Gets or sets the text displayed in the bottom left status label
    ''' </summary>
    Public Property StatusLabelText As String

    ''' <summary>
    ''' The text to display in the log TextBlock
    ''' </summary>
    Public Property LogText As String

    ''' <summary>
    ''' Gets the default baud rate collection
    ''' </summary>
    Public ReadOnly Property BaudRates As Integer()
        Get
            Return mySerialSettings.BaudRates
        End Get
    End Property

    ''' <summary>
    ''' Gets the default Baud Rate
    ''' </summary>
    Public ReadOnly Property DefaultBaudRate As Integer
        Get
            Return mySerialSettings.BaudRate
        End Get
    End Property

    ''' <summary>
    ''' Gets the default StopBits value.
    ''' </summary>
    Public ReadOnly Property StopBits As StopBits
        Get
            Return mySerialSettings.StopBits
        End Get
    End Property

    ''' <summary>
    ''' Gets the default Parity value.
    ''' </summary>
    Public ReadOnly Property Parity As Parity
        Get
            Return mySerialSettings.Parity
        End Get
    End Property

    ''' <summary>
    ''' Gets the default data bits value.
    ''' </summary>
    Public ReadOnly Property DataBits As Integer
        Get
            Return mySerialSettings.DataBits
        End Get
    End Property

    Private portList As List(Of String)


    ''' <summary>
    ''' Gets or sets the available ports on the computer
    ''' </summary>
    Public ReadOnly Property PortNameCollection() As String()
        Get
            If Not IsNothing(portList) Then
                Return portList.ToArray
            Else
                portList = SerialPort.GetPortNames.ToList
                CheckForValidCOM()
                Return portList.ToArray
            End If
            Return SerialPort.GetPortNames()
        End Get
    End Property

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
                portList.Remove(port)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Gets or sets the selected baud rate
    ''' </summary>
    Public Property BaudRate As Integer

    ''' <summary>
    ''' Gets or sets the selected name of the port to use for the GPS
    ''' </summary>
    Public Property PortName As String

    ''' <summary>
    ''' Gets or sets the Longitude value
    ''' </summary>
    Public Property Longitude As String

    ''' <summary>
    ''' Gets or sets the Latitude value
    ''' </summary>
    Public Property Latitude As String

    ''' <summary>
    ''' Gets or sets the Satellites In View value
    ''' </summary>
    Public Property SatsInView As String

    ''' <summary>
    ''' Gets or sets the Satellites In Use value
    ''' </summary>
    Public Property SatsInUse As String

    ''' <summary>
    ''' Gets or sets the Fix Mode value
    ''' </summary>
    Public Property FixMode As String

    ''' <summary>
    ''' Gets or sets the GPS Local Time value
    ''' </summary>
    Public Property LocalTime As String

    ''' <summary>
    ''' Gets or sets the GPS Status value
    ''' </summary>
    Public Property Status As String

End Class
