Class Parser
	''' <summary>
	''' Naparsruje příkazy a vrátí je v poli, zde NEprobíhá validace.
	''' Oddělovače jsou střední (;) | mezera ( ) | enter ()
	''' </summary>
	''' <param name="prikazy">Stríng příkazů</param>
	''' <returns>Pole příkazů</returns>
	Public Function ParsujPrikazy(prikazy As String) As String()
		' Rozdělíme string a smažeme prázdné záznamy
		Return prikazy.Split(New Char() {" ", ";", vbCrLf},
							 System.StringSplitOptions.RemoveEmptyEntries)
	End Function

End Class
