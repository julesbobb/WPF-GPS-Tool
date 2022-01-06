
Namespace Classes

    ''' <summary>
    ''' The Class used to collect the SNR information for the 12 satellites. Used for graph and radial chart
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ChartDataProperties

        ''' <summary>
        ''' The satellite number 
        ''' </summary>
        Property SatalliteNumber As Int16

        ''' <summary>
        ''' Signal to Noise Ratio from 0-99
        ''' </summary>
        Property SNR As Int16 = 0

        ''' <summary>
        ''' Degrees from true North. Measured 0-99
        ''' </summary>
        Property Azimuth As Int16 = 0

        ''' <summary>
        ''' Elevation in degrees, measured 0-90
        ''' </summary>
        Property Elevation As Int16 = 0

        ''' <summary>
        ''' Whether the satallite has a fix. If so then the back color of the bar is set to Green. If not then set to Red
        ''' </summary>
        Property HasFix As Boolean

    End Class


End Namespace