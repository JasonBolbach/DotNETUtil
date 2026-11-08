Public Class GZipAssist
    Implements iCompressionAssist
    Public Function CompressToBytes(ByVal Data As DataSet) As Byte() Implements iCompressionAssist.CompressToBytes
        Using msCompressed As New IO.MemoryStream()
            Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                Data.WriteXml(GZ, XmlWriteMode.WriteSchema)
                GZ.Flush() : GZ.Close()
            End Using
            Return msCompressed.ToArray()
        End Using
    End Function
    Public Function CompressToString(ByVal Data As DataSet) As String Implements iCompressionAssist.CompressToString
        Dim bCompressed() As Byte = CompressToBytes(Data)
        Dim sCompressed As String = Convert.ToBase64String(bCompressed)

        Try
            Return sCompressed
        Catch ex As Exception
            Throw ex
            Return ""
        Finally
            Erase bCompressed
            sCompressed = ""
        End Try
    End Function

    Public Function CompressToBytes(ByVal Data As IO.Stream) As Byte() Implements iCompressionAssist.CompressToBytes
        Using msCompressed As New IO.MemoryStream()
            Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                Data.CopyTo(GZ)
                GZ.Flush() : GZ.Close()
            End Using
            Return msCompressed.ToArray()
        End Using
    End Function
    Public Function CompressToString(ByVal Data As IO.Stream) As String Implements iCompressionAssist.CompressToString
        Dim bCompressed() As Byte = CompressToBytes(Data)
        Dim sCompressed As String = Convert.ToBase64String(bCompressed)

        Try
            Return sCompressed
        Catch ex As Exception
            Throw ex
            Return ""
        Finally
            Erase bCompressed
            sCompressed = ""
        End Try
    End Function

    Public Function CompressToBytes(ByVal Data() As Byte) As Byte() Implements iCompressionAssist.CompressToBytes
        Using msCompressed As New IO.MemoryStream()
            Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                GZ.Write(Data, 0, Data.Length)
                GZ.Flush() : GZ.Close()
            End Using
            Return msCompressed.ToArray()
        End Using
    End Function
    Public Function CompressToString(ByVal Data() As Byte) As String Implements iCompressionAssist.CompressToString
        Dim bCompressed() As Byte = CompressToBytes(Data)
        Dim sCompressed As String = Convert.ToBase64String(bCompressed)

        Try
            Return sCompressed
        Catch ex As Exception
            Throw ex
            Return ""
        Finally
            Erase bCompressed
            sCompressed = ""
        End Try
    End Function

    Public Function CompressToBytes(ByVal Data As String) As Byte() Implements iCompressionAssist.CompressToBytes
        Dim bData() As Byte = Text.Encoding.ASCII.GetBytes(Data)
        Try
            Return CompressToBytes(bData)
        Catch ex As Exception
            Throw ex
            Return Nothing
        Finally
            Erase bData
        End Try
    End Function
    Public Function CompressToString(ByVal Data As String) As String Implements iCompressionAssist.CompressToString
        Dim bCompressed() As Byte = CompressToBytes(Data)
        Dim sCompressed As String = Convert.ToBase64String(bCompressed)

        Try
            Return sCompressed
        Catch ex As Exception
            Throw ex
            Return ""
        Finally
            Erase bCompressed
            sCompressed = ""
        End Try
    End Function

    Public Function DecompressToBytes(ByVal Compressed() As Byte) As Byte() Implements iCompressionAssist.DecompressToBytes
        Using msSmall As New IO.MemoryStream(Compressed)
            Using msBig As New IO.MemoryStream
                Using GZ As New IO.Compression.GZipStream(msSmall, IO.Compression.CompressionMode.Decompress)
                    GZ.CopyTo(msBig)
                    GZ.Flush() : GZ.Close()
                End Using
                Return msBig.ToArray
            End Using
        End Using
    End Function
    Public Function DecompressToString(ByVal Compressed() As Byte) As String Implements iCompressionAssist.DecompressToString
        Dim bDecompressed() As Byte = DecompressToBytes(Compressed)

        Try
            Return Text.Encoding.ASCII.GetString(bDecompressed)
        Catch ex As Exception
            Throw ex
            Return ""
        Finally
            Erase bDecompressed
        End Try
    End Function

    Public Function DecompressToBytes(ByVal Compressed As String) As Byte() Implements iCompressionAssist.DecompressToBytes
        Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
        Dim bDecompressed() As Byte = DecompressToBytes(bCompressed)
        Try
            Return bDecompressed
        Catch ex As Exception
            Throw ex
            Return Nothing
        Finally
            Erase bCompressed, bDecompressed
        End Try
    End Function
    Public Function DecompressToString(ByVal Compressed As String) As String Implements iCompressionAssist.DecompressToString
        Dim bDecompressed() As Byte = DecompressToBytes(Compressed)
        Dim sDecompressed As String = System.Text.Encoding.ASCII.GetString(bDecompressed)
        Try
            Return sDecompressed
        Catch ex As Exception
            Throw ex
            Return Nothing
        Finally
            sDecompressed = ""
            Erase bDecompressed
        End Try
    End Function

    Public Sub DecompressToStream(ByVal Compressed As String, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
        Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
        Try
            DecompressToStream(bCompressed, Output)
        Catch ex As Exception
        Finally
            Erase bCompressed
        End Try
    End Sub
    Public Sub DecompressToStream(ByVal Compressed() As Byte, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
        Using msSmall As New IO.MemoryStream(Compressed)
            Using GZ As New IO.Compression.GZipStream(msSmall, IO.Compression.CompressionMode.Decompress)
                GZ.CopyTo(Output)
                GZ.Flush() : GZ.Close()
            End Using
        End Using
    End Sub

    Public Sub DecompressToDataSet(ByVal Compressed As String, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
        Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
        Try
            DecompressToDataSet(bCompressed, DS)
        Catch ex As Exception
        Finally
            Erase bCompressed
        End Try
    End Sub
    Public Sub DecompressToDataSet(ByVal Compressed() As Byte, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
        Using msCompressed As New IO.MemoryStream(Compressed)
            Using msBig As New IO.MemoryStream
                Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Decompress)
                    GZ.CopyTo(msBig)
                    GZ.Flush() : GZ.Close()
                End Using
                msBig.Seek(0, IO.SeekOrigin.Begin)
                DS.ReadXml(msBig)
            End Using
        End Using
    End Sub
End Class

