<script type="text/javascript">
<!--
// Parses the xmlResponse from status.xml and updates the status box
function updateStatus(xmlData) {
	var mainstat = document.getElementById('display').style.display;
	var loadstat = document.getElementById('loading').style.display;

	// Check if a timeout occurred
	if(!xmlData)
	{
		mainstat = 'none';
		loadstat = 'inline';
		return;
	}

	// Make sure we're displaying the status display
	mainstat = 'inline';
	loadstat = 'none';

	// Loop over all the LEDs
	for(i = 0; i < 1; i++) // Temporarily change 3 to 1
		document.getElementById('led' + i).style.color = (getXMLValue(xmlData, 'led' + i) == '1') ? '#090' : '#ddd';

	// Loop over all the buttons
	for(i = 0; i < 3; i++)
		document.getElementById('btn' + i).innerHTML = (getXMLValue(xmlData, 'btn' + i) == 'up') ? '&Lambda;' : 'V';

	// Update the POT value
	document.getElementById('pot0').innerHTML = getXMLValue(xmlData, 'pot0');
}
setTimeout("newAJAXCommand('status.xml', updateStatus, true)",500);
//-->
</script>

<script type="text/javascript">
<!--
document.getElementById('hello').innerHTML = "~hellomsg~";
//-->
</script>