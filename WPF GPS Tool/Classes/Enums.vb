Namespace Classes

    Public Module Enums

        Public Enum PerformCheck
            ''' <summary>
            ''' Check for a 3D mode signal on the GPGSA stream and that an SNR is detect simultaneously on 3 satellites 
            ''' </summary>
            ''' <remarks></remarks>
            GPS_TEST_ONLY = 1
            GPS_AND_FORCE_STATIC = 2
            GPS_AND_DISBALE_FORCE_STATIC = 3
            DETECT_GPS_ONLY = 4
            ''' <summary>
            ''' Check for a 3D mode signal on the GPGSA
            ''' </summary>
            ''' <remarks></remarks>
            GPS_3DFIX_ONLY = 5
        End Enum

        'OUT PARAMETERS (Exit codes)
        '0 - User ended application manually (FAIL)
        '1 - No GPS detected
        '4 - GPS detected
        '5 - GPS detected and min 3 sat SNR OK
        '6 - GPS detected and static forced ON
        '7 - GPS detected and static forced OFF
        '8 - GPS detected and FIX OK
        Public Enum OutputCode

            ''' <summary>
            ''' User ended application manually (FAIL)
            ''' </summary>
            ''' <remarks></remarks>
            FAIL_USER_ENDED_APP_MANUALLY = 0

            ''' <summary>
            ''' No GPS detected
            ''' </summary>
            NO_GPS_DETECTED = 1

            ''' <summary>
            ''' GPS detected
            ''' </summary>
            GPS_DETECTED = 4

            ''' <summary>
            '''  GPS detected and min 3 sat SNR OK
            ''' </summary>
            GPS_DETECTED_MIN_SNR = 5

            ''' <summary>
            ''' GPS detected and static forced ON
            ''' </summary>
            GPS_DETECTED_STATIC_FORCED_ON = 6

            ''' <summary>
            ''' GPS detected and static forced OFF
            ''' </summary>
            GPS_DETECTED_STATIC_FORCED_OFF = 7

            ''' <summary>
            ''' GPS detected and FIX OK
            ''' </summary>
            GPS_DETECTED_FIX_OK = 8
        End Enum

        ''' <summary>
        ''' The current state of the serial port connection
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum SerialConnectionState
            ''' <summary>
            ''' Port is currently open
            ''' </summary>
            Open

            ''' <summary>
            ''' Port is currently closed
            ''' </summary>
            Closed
        End Enum

        ''' <summary>
        ''' The method the user is using to connection to the COM port
        ''' </summary>
        Public Enum ConnectionType
            ''' <summary>
            ''' Connecting to an individual port
            ''' </summary>
            Manual

            ''' <summary>
            ''' Running an automatic scan of all available ports
            ''' </summary>
            AutoScan
        End Enum

    End Module

End Namespace


