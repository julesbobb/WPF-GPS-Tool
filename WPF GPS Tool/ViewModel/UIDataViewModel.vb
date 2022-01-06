Imports Classes
Imports Classes.Events

Public Class UIDataViewModel
    Inherits ViewModelBase

    Private _UIData As UIData

    Public Sub New()
        Me._UIData = New UIData With {.AutoScanEnabled = True,
                                    .StartButtonContent = "Connect",
                                    .StatusLabelText = "Not Connected"
                                   }
    End Sub

    Public Property grpBoxSettingsIsEnabled() As Boolean
        Get
            Return _UIData.grpBoxSettingsIsEnabled
        End Get
        Set(ByVal value As Boolean)
            _UIData.grpBoxSettingsIsEnabled = value
            MyBase.OnPropertyChanged("grpBoxSettingsIsEnabled")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the number of satalites with a fix
    ''' </summary>
    Public Property SatsWithFix As Integer
        Get
            Return _UIData.SatsWithFix
        End Get
        Set(value As Integer)
            _UIData.SatsWithFix = value
            MyBase.OnPropertyChanged("SatsWithFix")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the number of satalites that have reached the minimum signal strength
    ''' </summary>
    Public Property SatsWithRequiredStrength As Integer
        Get
            Return _UIData.SatsWithRequiredStrength
        End Get
        Set(value As Integer)
            _UIData.SatsWithRequiredStrength = value
            MyBase.OnPropertyChanged("SatsWithRequiredStrength")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the Longitude value
    ''' </summary>
    Public Property Longitude As String
        Get
            Return _UIData.Longitude
        End Get
        Set(value As String)
            _UIData.Longitude = value
            MyBase.OnPropertyChanged("Longitude")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the Latitude value
    ''' </summary>
    Public Property Latitude As String
        Get
            Return _UIData.Latitude
        End Get
        Set(value As String)
            _UIData.Latitude = value
            MyBase.OnPropertyChanged("Latitude")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the Satalites In View value
    ''' </summary>
    Public Property SatsInView As String
        Get
            Return _UIData.SatsInView
        End Get
        Set(value As String)
            _UIData.SatsInView = value
            MyBase.OnPropertyChanged("SatsInView")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the Satalites In Use value
    ''' </summary>
    Public Property SatsInUse As String
        Get
            Return _UIData.SatsInUse
        End Get
        Set(value As String)
            _UIData.SatsInUse = value
            MyBase.OnPropertyChanged("SatsInUse")
        End Set
    End Property

    ''' <summary>
    ''' Gets the available ports on the computer
    ''' </summary>
    Public ReadOnly Property PortNameCollection() As String()
        Get
            Return _UIData.PortNameCollection
        End Get
    End Property

    Private _CurrentPort As String
    ''' <summary>
    ''' Gets or sets the currently selected Port
    ''' </summary>
    Public Property CurrentPort() As String
        Get
            Return _CurrentPort
        End Get
        Set(ByVal value As String)
            _CurrentPort = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the baud rate collection
    ''' </summary>
    Public ReadOnly Property BaudRates As Integer()
        Get
            Return _UIData.BaudRates
        End Get
    End Property

    Public ReadOnly Property DefaultBaudRate As Integer
        Get
            Return _UIData.DefaultBaudRate
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the Fix Mode value
    ''' </summary>
    Public Property FixMode As String
        Get
            Return _UIData.FixMode
        End Get
        Set(value As String)
            _UIData.FixMode = value
            MyBase.OnPropertyChanged("FixMode")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the GPS Local Time value
    ''' </summary>
    Public Property LocalTime As String
        Get
            Return _UIData.LocalTime
        End Get
        Set(value As String)
            _UIData.LocalTime = value
            MyBase.OnPropertyChanged("LocalTime")
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the GPS Status value
    ''' </summary>
    Public Property Status As String
        Get
            Return _UIData.Status
        End Get
        Set(value As String)
            If value <> _UIData.Status Then
                _UIData.Status = value
                MyBase.OnPropertyChanged("Status")
            End If
        End Set
    End Property

    ''' <summary>
    ''' The selected baud rate
    ''' </summary>
    Public Property BaudRate() As Integer
        Get
            Return _UIData.BaudRate
        End Get
        Set(value As Integer)
            If _UIData.BaudRate <> value Then
                _UIData.BaudRate = value
                MyBase.OnPropertyChanged("BaudRate")
            End If
        End Set
    End Property

    ''' <summary>
    ''' The text displayed on the manual start button 
    ''' </summary>
    Public Property StartButtonContext() As String
        Get
            Return _UIData.StartButtonContent
        End Get
        Set(ByVal value As String)
            _UIData.StartButtonContent = value
            MyBase.OnPropertyChanged("StartButtonContext")
        End Set
    End Property

    ''' <summary>
    ''' The IsEnabled status of the AutoScan button
    ''' </summary>
    Public Property AutoScanEnabled() As Boolean
        Get
            Return _UIData.AutoScanEnabled
        End Get
        Set(ByVal value As Boolean)
            _UIData.AutoScanEnabled = value
            MyBase.OnPropertyChanged("AutoScanEnabled")
        End Set
    End Property

    ''' <summary>
    ''' The text displayed in the bottom left status label
    ''' </summary>
    Public Property StatusLabelText() As String
        Get
            Return _UIData.StatusLabelText
        End Get
        Set(ByVal value As String)
            _UIData.StatusLabelText = value
            MyBase.OnPropertyChanged("StatusLabelText")
        End Set
    End Property

    ''' <summary>
    ''' The text to display in the log TextBlock
    ''' </summary>
    Public Property LogText As String
        Get
            Return _UIData.LogText
        End Get
        Set(value As String)
            _UIData.LogText = value
            MyBase.OnPropertyChanged("LogText")
        End Set
    End Property

    Friend Sub UpdateConnectButtons(e As ConnectionStateEventsArgs)
        Select Case e.State
            Case SerialConnectionState.Open
                Me.StartButtonContext = "Disconnect"
                Me.AutoScanEnabled = False
            Case SerialConnectionState.Closed
                Me.StartButtonContext = "Connect"
                Me.AutoScanEnabled = True
        End Select
    End Sub

    Friend Sub UpdateStatusLabelHander(e As UpdateEventsArgs)
        StatusLabelText = e.LogText
    End Sub

End Class
