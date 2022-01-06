Imports System.Globalization
Imports Classes.Events

Namespace Classes

    Public Class GPSLine

#Region "Fields"

        Friend Shared lstSNRS As New List(Of GPGSV_Signals)

        'The standard culture used for GPS 
        Dim GPSCulture As New CultureInfo("en-US")
        Friend Event IsGPSSignal(ByVal value As Boolean)
        Public Shared IsGPS As Boolean = False

        Const iMinSignalStrengthRequired As Integer = 30
        Const iFixedSatsRequired As Int16 = 4

        Private portMan As PortManager
        Shared pUIViewModel As UIDataViewModel

        Public Sub New(portMan As PortManager, vUIViewModel As UIDataViewModel)
            Me.portMan = portMan
            pUIViewModel = vUIViewModel

            If lstSNRS.Count = 0 Then
                For i As Int16 = 0 To 11
                    lstSNRS.Add(New GPGSV_Signals)
                Next
            End If

        End Sub

        ''' <summary>
        ''' Used to monitor the number of GPGSV (Satalites In View) messages in the cycle. This determines the column count in the chart
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared GPGSVCycleCount As Integer = 0

#End Region

#Region "Exposed Methods"

        ''' <summary>
        ''' Confirm that the ReadLine matches the checksum value. This is passed along with every GPS signal 
        ''' for use in validation. If it fails then the signal is not decoded.
        ''' </summary>
        ''' <param name="ReadLine">The individual ReadLine data from the serial port Data_Recieved event handler</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function IsValidCheckSum(ReadLine As String) As Boolean
            Dim count As Integer
            For Each character As Char In ReadLine
                Select Case character
                    Case "$"c
                        ' Ignore the dollar sign
                    Case "*"c
                        ' Stop processing before the asterisk
                        Exit For
                    Case Else
                        ' Is this the first value for the checksum?
                        If count = 0 Then
                            ' Yes. Set the checksum to the value
                            count = Convert.ToByte(character)
                        Else
                            ' No. XOR the checksum with this character's value
                            count = count Xor Convert.ToByte(character)
                        End If
                End Select
            Next
            Return ReadLine.Trim.Substring(ReadLine.IndexOf("*") + 1) = count.ToString("X2")
        End Function

        Friend Sub ResetGPSBoolean()
            IsGPS = False
        End Sub

        ''' <summary>
        ''' Decode in the incoming stream of GPS text. Call the appropriate routines for each i.e. $GPRMC, $GPGSV, $GPGSA and $GPGGA. These will 
        ''' in turn fire the events which alter the UI 
        ''' </summary>
        ''' <param name="signal">GPS received input line</param>
        ''' <remarks></remarks>
        Friend Sub DecodeSignal(signal As String)

            If IsGPS = False Then
                VerifyGPSFormat(signal)
            End If

            Dim line As String = RemoveCheckSum(signal)

            Select Case SplitWords(line)(0)
                Case "$GPRMC", "$GNRMC"
                    DecodeGPRMC(line)
                Case "$GPGSV"
                    ParseGPGSV(line)
                    '  DecodeGPGSV(line)
                Case "$GPGSA", "$GNGSA"
                    'GPS DOP and active satellites
                    DecodeGPGSA(line)
                Case "$GPGGA", "$GNGGA"
                    DecodeGPGGA(line)
            End Select
        End Sub

        ''' <summary>
        ''' GPS DOP and active satellites. Update the list of the satalites that have a fix, the satalite status and 3D fix
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub DecodeGPGSA(line As String)
            ' Divide the sentence into words
            Dim Words() As String = SplitWords(line)

            If Words(2) <> "" Then
                Select Case Convert.ToDouble(Words(2))
                    Case 1
                        pUIViewModel.FixMode = "Fix not available"
                    Case 2
                        pUIViewModel.FixMode = "2D"
                    Case 3
                        pUIViewModel.FixMode = "3D"
                        If portMan.ProvideOutput And portMan.CheckToPerform = PerformCheck.GPS_3DFIX_ONLY Then
                            portMan.RaiseWriteOutput(OutputCode.GPS_DETECTED_FIX_OK)
                        End If
                End Select

                For Each signal As GPGSV_Signals In lstSNRS
                    signal.HasFix = False
                Next

                '3-14 = IDs of SVs used in position fix (null for unused fields)
                For i As Int16 = 3 To 14
                    If Words(i).Length > 0 Then
                        For Each signal As GPGSV_Signals In lstSNRS
                            If Words(i) = signal.SatalliteNumber Then
                                signal.HasFix = True
                            End If
                        Next
                    End If
                Next
            End If
        End Sub

#End Region

#Region "Private Methods"

        Private Function RemoveCheckSum(line As String) As String
            Dim array As String() = line.TrimEnd.Split("*")
            Return array(0)
        End Function

        ''' <summary>
        ''' Divides a sentence into individual words
        ''' </summary>
        ''' <param name="sentence"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function SplitWords(ByVal sentence As String) As String()
            Return sentence.Split(","c)
        End Function

        ''' <summary>
        ''' Reads the text line to determine if it matches the criteria for a GPS signal
        ''' </summary>
        ''' <param name="line">The ReadLine from the data received event</param>
        Private Sub VerifyGPSFormat(line As String)
            'Discard the sentence any sentences that do not begin with a $
            '3 seconds attempts to analyze individual lines. That should be enough to discount
            'the possibility of non-dollar lines and to have at least one valid line. If one hasn't been found by then
            'then it has failed

            If line.Substring(0) <> "$" Then
                If line.Substring(1, 2) = "GP" Or line.Substring(1, 2) = "GN" Or line.Substring(1, 2) = "GL" Then
                    IsGPS = True
                    RaiseEvent IsGPSSignal(True)
                End If
            End If
        End Sub

#Region "Decode Line Types"

        ''' <summary>
        ''' Recommended minimum specific GPS/Transit data 
        ''' </summary>
        ''' <param name="sentence"></param>
        ''' <remarks></remarks>
        Private Sub DecodeGPRMC(ByVal sentence As String)
            ' Divide the sentence into words
            Dim Words() As String = SplitWords(sentence)
            ' Do we have enough values to describe our location?
            If Words(3) <> "" And Words(4) <> "" _
            And Words(5) <> "" And Words(6) <> "" Then
                ' Extract latitude and longitude
                ' Append hours
                Dim Latitude As String = Words(3).Substring(0, 2) & "°"
                ' Append minutes
                Latitude = String.Format("{0}{1}""", Latitude, Words(3).Substring(2))
                ' Append the hemisphere
                Latitude = Latitude & Words(4)
                ' Append hours
                Dim Longitude As String = Words(5).Substring(0, 3) & "°"
                ' Append minutes
                Longitude = String.Format("{0}{1}""", Longitude, Words(5).Substring(3))

                ' Append the hemisphere
                Longitude = Longitude & Words(6)
                ' Notify the calling application of the change

                pUIViewModel.Longitude = Longitude
                pUIViewModel.Latitude = Latitude

            End If
            ' Do we have enough values to parse satellite-derived time?
            If Words(1) <> "" Then
                ' Yes. Extract hours, minutes, seconds and milliseconds
                Dim Hours As Integer = CType(Words(1).Substring(0, 2), Integer)
                Dim Minutes As Integer = CType(Words(1).Substring(2, 2), Integer)
                Dim Seconds As Integer = CType(Words(1).Substring(4, 2), Integer)
                Dim Milliseconds As Integer
                ' Extract milliseconds if it is available
                If Words(1).Length > 7 Then Milliseconds = _
                  CType(Single.Parse(Words(1).Substring(6), GPSCulture) * 1000, Integer)
                ' Now build a DateTime object with all values
                Dim Today As DateTime = DateTime.Now.ToUniversalTime
                Dim SatelliteTime As New DateTime(Today.Year, Today.Month,
                  Today.Day, Hours, Minutes, Seconds,
                  Milliseconds)
                ' Notify of the new time, adjusted to the local time zone
                pUIViewModel.LocalTime = SatelliteTime.ToLocalTime.ToString
            End If
            ' Does the device currently have a satellite fix?
            If Words(2) <> "" Then
                Select Case Words(2)
                    Case "A"
                        pUIViewModel.Status = "Fix Obtained"

                    Case "V"
                        pUIViewModel.Status = "Fix Lost"
                End Select
            End If
            ' Indicate that the sentence was recognized
        End Sub

        ''' <summary>
        ''' The Global Positioning System Fix Data. Time, position and fix related data for a GPS receiver. 
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub DecodeGPGGA(line As String)
            ' Divide the sentence into words
            Dim Words() As String = SplitWords(line)

            If Words(7) <> "" Then
                pUIViewModel.SatsInUse = CType(Words(7), Integer).ToString
            End If
        End Sub

        ' Divides a sentence into individual words
        Public Function GetWords(ByVal sentence As String) As String()
            Return sentence.Split(","c)
        End Function

        Dim iNumberOfSentencesInGSV As Integer
        Dim iCurrentSentenceNumber As Integer
        Private Shared iFixCount As Integer
        Private Shared iWithRequiredStrength As Integer

        Public Function ParseGPGSV(ByVal sentence As String) As Boolean
            Dim PseudoRandomCode As Integer
            Dim Azimuth As Integer
            Dim Elevation As Integer
            Dim SignalToNoiseRatio As Integer

            ' Divide the sentence into words
            Dim Words() As String = GetWords(sentence)
            ' Each sentence contains four blocks of satellite information.
            ' Read each block and report each satellite's information

            iNumberOfSentencesInGSV = CType(Words(1), Integer)
            iCurrentSentenceNumber = CType(Words(2), Integer)

            Dim Count As Integer
            pUIViewModel.SatsInView = CType(Words(3), Integer)

            If iCurrentSentenceNumber = 1 Then
                'reset fix results
                iFixCount = 0
                iWithRequiredStrength = 0
            End If

            For Count = 1 To 4
                ' Does the sentence have enough words to analyze?
                If (Words.Length - 1) >= (Count * 4 + 3) Then
                    ' Yes.  Proceed with analyzing the block.  Does it contain any
                    ' information?
                    If Words(Count * 4) <> "" And Words(Count * 4 + 1) <> "" _
                    And Words(Count * 4 + 2) <> "" And Words(Count * 4 + 3) <> "" Then
                        ' Yes. Extract satellite information and report it
                        'reset all
                        PseudoRandomCode = 0
                        Azimuth = 0
                        Elevation = 0
                        SignalToNoiseRatio = 0

                        PseudoRandomCode = CType(Words(Count * 4), Integer)
                        Elevation = CType(Words(Count * 4 + 1), Integer)
                        Azimuth = CType(Words(Count * 4 + 2), Integer)
                        SignalToNoiseRatio = CType(Words(Count * 4 + 3), Integer)

                        If SignalToNoiseRatio >= iMinSignalStrengthRequired Then
                            iWithRequiredStrength += 1
                        End If
                        iFixCount += 1
                    End If
                End If
            Next

            If iCurrentSentenceNumber = iNumberOfSentencesInGSV Then
                'the last sentence in the group 
                'therefore, display the result
                If iFixCount > 0 Then
                    pUIViewModel.SatsWithFix = iFixCount.ToString
                Else
                    pUIViewModel.SatsWithFix = String.Empty
                End If
                If iWithRequiredStrength > 0 Then
                    pUIViewModel.SatsWithRequiredStrength = iWithRequiredStrength.ToString
                Else
                    pUIViewModel.SatsWithRequiredStrength = String.Empty
                End If

            End If

            If portMan.ProvideOutput = True Then
                If iWithRequiredStrength >= iFixedSatsRequired Then
                    Select Case portMan.CheckToPerform
                        Case PerformCheck.GPS_AND_FORCE_STATIC
                            portMan.PerformForceStatic()
                        Case PerformCheck.GPS_AND_DISBALE_FORCE_STATIC
                            portMan.PerformDisableForceStatic()
                        Case PerformCheck.GPS_TEST_ONLY
                            portMan.RaiseWriteOutput(OutputCode.GPS_DETECTED_MIN_SNR)
                    End Select
                End If
            End If

            Return True

        End Function

#End Region

#End Region

    End Class

End Namespace

