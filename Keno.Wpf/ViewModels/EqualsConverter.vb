Imports System.Globalization

''' <summary>Binds a RadioButton to an integer property: checked when the value equals the converter parameter.</summary>
Public Class EqualsConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Return value IsNot Nothing AndAlso String.Equals(value.ToString(), parameter?.ToString(), StringComparison.Ordinal)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If TypeOf value Is Boolean AndAlso CBool(value) AndAlso parameter IsNot Nothing Then
            Return Integer.Parse(parameter.ToString(), CultureInfo.InvariantCulture)
        End If
        Return Binding.DoNothing
    End Function

End Class
