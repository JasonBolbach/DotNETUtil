''' <summary>
''' An interface that allows for generic compression assistants.
''' </summary>
''' <remarks></remarks>
Public Interface iCompressionAssist
    Function CompressToBytes(ByVal Data As DataSet) As Byte()
    Function CompressToString(ByVal Data As DataSet) As String

    Function CompressToBytes(ByVal Data As IO.Stream) As Byte()
    Function CompressToString(ByVal Data As IO.Stream) As String

    Function CompressToBytes(ByVal Data() As Byte) As Byte()
    Function CompressToString(ByVal Data() As Byte) As String

    Function CompressToBytes(ByVal Data As String) As Byte()
    Function CompressToString(ByVal Data As String) As String

    Function DecompressToBytes(ByVal Compressed() As Byte) As Byte()
    Function DecompressToString(ByVal Compressed() As Byte) As String

    Function DecompressToBytes(ByVal Compressed As String) As Byte()
    Function DecompressToString(ByVal Compressed As String) As String

    Sub DecompressToStream(ByVal Compressed As String, ByVal Output As IO.Stream)
    Sub DecompressToStream(ByVal Compressed() As Byte, ByVal Output As IO.Stream)

    Sub DecompressToDataSet(ByVal Compressed As String, ByRef DS As DataSet)
    Sub DecompressToDataSet(ByVal Compressed() As Byte, ByRef DS As DataSet)
End Interface

