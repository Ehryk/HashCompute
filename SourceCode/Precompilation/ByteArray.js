var fs = require('fs');

var type = process.argv[2];
var language = process.argv[3];
var full = process.argv[4];

var namespace = "Core";
var classname = "Precompiled";
var newline = "\r\n"; //Windows

var array = [];
var content = "";
var extension = "";

function generateArray(type)
{
  for(var i = 0; i < 256; i++)
  {
  	if (dimensions(type) == 2) {
	  	array[i] = [];
	  	for(var j = 0; j < 256; j++)
	  		array[i][j] = getValue(type, i, j);
	}
	else {
		array[i] = getValue(type, i);
	}
  }
}

function generateContent(type, language, full) {
	switch(language.toUpperCase())
	{
		case "CSHARP":
		case "CS":
		case "C#":
  			generateCS(type, full);
  			break;

		default:
			throw new Error("Language '{0}' not recognized".format(language));
	}
}

function generateCS(type, full) {
  content = "";
  if (isTrue(full))
  {
  	content += "//Generated by Precompilation/ByteArray.js{2}{2}namespace {0}{2}{{2}    public static partial class {1}{2}    {{2}".format(namespace, classname, newline);
  }

  if (dimensions(type) == 2)
  {
  	  //Two Dimensional
	  content += "{0}public static byte[,] {1} = new byte[256,256]{2}".format(isTrue(full) ? "        " : "", type, newline);
	  content += "{0}{{1}".format(isTrue(full) ? "        " : "    ", newline);
	  for(var i = 0; i < array.length; i++)
	  {
	  	content += "{0}{ ".format(isTrue(full) ? "            " : "    ");
	  	for(var j = 0; j < array[i].length; j++)
	  	{
	  		var lastItem = j == array[j].length - 1;
	  		content += "0x{0}{1}".format(getHexByte(array[i][j]), lastItem ? "" : ", ");
	  	}
	  	var lastItem = i == array[i].length - 1;
	  	content += " }{0} //{1}{2}".format(lastItem ? "" : ",", i,  newline);
	  }
	  content += "{0}};".format(isTrue(full) ? "        " : "", type);
  }
  else
  {
  	  //One Dimensional
	  content += "{0}public static byte[] {1} = new byte[256]{2}".format(isTrue(full) ? "        " : "", type, newline);
	  content += "{0}{{1}".format(isTrue(full) ? "        " : "    ", newline);
	  for(var i = 0; i < array.length; i++)
	  {
	  	content += "{0}".format(isTrue(full) ? "            " : "    ");
	  	content += "0x{0}".format(getHexByte(array[i]));
	  	var lastItem = i == array.length - 1;
	  	content += "{0} //{1}{2}".format(lastItem ? "" : ",", i, newline);
	  }
	  content += "{0}};".format(isTrue(full) ? "        " : "", type);
  }

  if (isTrue(full))
  {
  	content += "{0}    }{0}}".format(newline);
  }

  content += "{0}{0}".format(newline);
}

function writeToFile() {
  fs.writeFile("{0}_{1}.{2}".format(classname, type, extension), content, function(err) {
    if(err) {
      return console.log(err);
    }
  }); 
}

function getExtension(language) {
  if (language.toLowerCase() == "cs")
  	extension = "cs";
}

function getValue(type, a, b) {
	switch(type.toUpperCase())
	{
		case "XOR":
			return a ^ b;
			
		case "AND":
			return a & b;

		case "NAND":
			return 255 - (a & b);

		case "OR":
			return a | b;
			
		case "NOR":
			return 255 - (a | b);
			
		case "XNOR":
			return 255 - (a ^ b);
			
		case "NOT":
			return 255 - a;
			
		case "INC":
			return (a + 1 == 256) ? 0 : a + 1;
			
		case "DEC":
			return (a == 0) ? 255 : a - 1;;
			
		case "REV":
			return  1 * (a >=128 ? 1 : 0) + 
					2 * (a % 128 >= 64 ? 1 : 0) + 
					4 * (a % 64 >= 32 ? 1 : 0) + 
					8 * (a % 32 >= 16 ? 1 : 0) + 
					16 * (a % 16 >= 8 ? 1 : 0) + 
					32 * (a % 8 >= 4 ? 1 : 0) + 
					64 * (a % 4 >= 2 ? 1 : 0) + 
					128 * (a % 2 == 1 ? 1 : 0);

		default:
			throw new Error("Type '{0}' not recognized".format(type));
	}
}

function dimensions(type) {
  if (["AND", "NAND", "OR", "NOR", "XOR", "XNOR"].indexOf(type) >= 0)
  	return 2;
  return 1;
}

//Helper Functions

function isTrue(value){
  if (typeof(value) == 'string'){
    value = value.toLowerCase();
  }
  switch(value){
    case true:
    case "true":
    case 1:
    case "1":
    case "on":
    case "yes":
      return true;
    default: 
      return false;
  }
}

if (!String.prototype.format) {
  String.prototype.format = function() {
    var args = arguments;
    return this.replace(/{(\d+)}/g, function(match, number) { 
      return typeof args[number] != 'undefined' ? args[number] : match;
    });
  };
}

function pad(n, width, z) {
  z = z || '0';
  n = n + '';
  return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
}

function getHexByte(value)
{
	return pad(value.toString(16), 2).toUpperCase();
}

generateArray(type);
generateContent(type, language, full);
getExtension(language);
writeToFile(type, extension);
