Imports System.ComponentModel
Imports System.Runtime.CompilerServices

''' <summary>Minimal INotifyPropertyChanged base for view models.</summary>
Public MustInherit Class ObservableObject
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub RaisePropertyChanged(name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

    Protected Function SetField(Of T)(ByRef field As T, value As T, <CallerMemberName> Optional name As String = Nothing) As Boolean
        If EqualityComparer(Of T).Default.Equals(field, value) Then Return False
        field = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        Return True
    End Function

End Class
