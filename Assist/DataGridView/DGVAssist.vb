Imports System.Windows.Forms
''' <summary>
''' Simplify DataRow searching, selection, and more in DataGridViews.
''' </summary>
''' <remarks>Assumes Guid Primary Key in table</remarks>
Public Class DGVAssist
    Public Shared Function GetRowIDByIndex(ByVal DGV As DataGridView, ByVal Index As Integer) As Guid
        If DGV Is Nothing Then Return Guid.Empty
        If Index >= DGV.Rows.Count Then Return Guid.Empty
        If Index < 0 Then Return Guid.Empty
        Dim Row As DataRow = GetRowByIndex(DGV, Index)
        If Row Is Nothing Then Return Guid.Empty
        Return Row.Item(Row.Table.PrimaryKey(0))
    End Function
    Public Shared Function GetRowByIndex(ByVal DGV As DataGridView, ByVal Index As Integer) As DataRow
        If DGV Is Nothing Then Return Nothing
        If Index >= DGV.Rows.Count Then Return Nothing
        If Index < 0 Then Return Nothing
        Return CType(DGV.Rows(Index).DataBoundItem, DataRowView).Row
    End Function

    Public Shared Function GetSelectedRowID(ByVal DGV As DataGridView) As Guid
        Dim R As DataRow = GetSelectedRow(DGV)
        If R Is Nothing Then Return Guid.Empty
        Return R.Item(R.Table.PrimaryKey(0))
    End Function
    Public Shared Function GetSelectedRow(ByVal DGV As DataGridView) As DataRow
        If DGV Is Nothing Then Return Nothing
        If DGV.SelectedRows.Count <= 0 Then Return Nothing
        Return CType(DGV.SelectedRows(0).DataBoundItem, DataRowView).Row
    End Function
    Public Shared Function GetSelectedRowIndex(ByVal DGV As DataGridView) As Integer
        If DGV Is Nothing Then Return Nothing
        If DGV.SelectedRows.Count <= 0 Then Return Nothing
        Return DGV.SelectedRows(0).Index
    End Function

    Public Shared Function GetDataRowFromRow(ByVal DGVR As Windows.Forms.DataGridViewRow) As DataRow
        If DGVR Is Nothing Then Return Nothing
        Try
            Return CType(DGVR.DataBoundItem, DataRowView).Row
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function FindRowByID(ByVal DGV As DataGridView, ByVal ID As Guid) As DataGridViewRow
        Dim DGVR, Result As DataGridViewRow
        Result = Nothing
        For Each DGVR In DGV.Rows
            Dim R As DataRow = CType(DGVR.DataBoundItem, DataRowView).Row
            If R.Item(R.Table.PrimaryKey(0)).ToString = ID.ToString Then
                Result = DGVR
                Exit For
            End If
        Next
        Return Result
    End Function
    Public Shared Sub SelectRow(ByVal DGV As DataGridView, ByVal ID As Guid)
        Dim DGVR, Temp As DataGridViewRow
        DGVR = FindRowByID(DGV, ID)
        For Each Temp In DGV.Rows
            Temp.Selected = False
        Next
        If DGVR Is Nothing Then Exit Sub
        DGVR.Selected = True
        DGV.FirstDisplayedScrollingRowIndex = DGVR.Index
    End Sub
    Public Shared Sub SelectRowAndEnsureVisible(ByVal DGV As DataGridView, ByVal ID As Guid)
        SelectRow(DGV, ID)
        Try
            DGV.FirstDisplayedScrollingRowIndex = DGV.SelectedRows(0).Index
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Returns the ID of the DataRow Above or Below the first currently selected row.
    ''' Useful for auto selecting the next available row upon deletion.
    ''' </summary>
    ''' <param name="DGV">The DataGridView</param>
    ''' <returns>The ID (Guid) of the row adjacent to the first currently selected row</returns>
    Public Shared Function GetAdjacentRowID(ByVal DGV As DataGridView, ByVal Index As Integer) As Guid
        Dim ID As Guid = GetRowIDByIndex(DGV, Index)
        If ID = Guid.Empty Then Return Guid.Empty
        If DGV.Rows.Count = 1 Then Return ID
        If Index = 0 Then Return GetRowIDByIndex(DGV, 1)
        Return GetRowIDByIndex(DGV, Index - 1)
    End Function

    Public Shared Sub StandardizeDGV(ByVal DGV As DataGridView)
        DGV.AllowUserToAddRows = False
        DGV.AllowUserToDeleteRows = False
        DGV.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.Aqua
        DGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DGV.RowHeadersVisible = True
    End Sub
End Class
