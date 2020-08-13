Imports System.Windows.Forms

Public Class Napoveda
	''' <summary>
	''' Položky nápovědy
	''' </summary>
	Private polozkyNapovedy As New Dictionary(Of String, String)

	Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
		Me.DialogResult = System.Windows.Forms.DialogResult.OK
		Me.Close()
	End Sub

	Private Sub napoveda_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		NactiPolozky()
	End Sub

	Private Sub NactiPolozky()
		Dim radky() As String = My.Resources.dokumentace.Split(vbCrLf)
		For Each radek As String In radky
			' pokud začíná křížkem je to komentář
			If Not radek.Trim().StartsWith("#") Then
				' Rozepberem to na části
				Dim casti() As String = radek.Split(":")
				Dim cast1 = casti(0).Trim()	' Odstraníme bílé znaky na začátku a konci
				Dim cast2 = casti(1).Trim()	' Odstraníme bílé znaky na začátku a konci
				polozkyNapovedy.Add(cast1, cast2) ' přídéme položku
			End If
		Next

		For Each polozka In polozkyNapovedy
			ListBox1.Items.Add(polozka.Key)
		Next
	End Sub

	Private Sub ListBox1_SelectedIndexChanged() Handles ListBox1.SelectedIndexChanged
		lblPopis.Text = polozkyNapovedy(ListBox1.SelectedItem).Trim()
	End Sub
End Class
