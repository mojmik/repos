<?php
class CustomFields {
  public $fieldsList=array();
  public function addField($c) {
    $this->fieldsList[] = $c;	  
  }
  public function getList() {
	return $this->fieldsList;
  }
  public function outFields() {
	foreach ($this->fieldsList as $f) {
	  if ($out!="") $out.="|";	
	  $out.=$f->outField();
	}
	return $out;
  }
   public function initFields(bool $doSave=true) {
	foreach ($this->fieldsList as $f) {
	  $out.="min:".$f->getValMin();
	  $out.="max:".$f->getValMax();
	  $out.="values:".$f->getValues();
	  if ($doSave) $f->save();
	}
	return $out;
  }
  public function getFields() {
	  return $this->fieldsList;
  }
  //todo load z table
  public function loadFromSQL() {
	global $wpdb;
	$query = "SELECT * FROM `".$wpdb->prefix."majax_fields`";
	$load=false;
	foreach( $wpdb->get_results($query) as $key => $row) {
		// each column in your row will be accessible like this
		//echo "<br />".$row->name;
		$this->fieldsList[] = new CustomField($row->name,$row->value,$row->type,$row->compare,$row->valMin,$row->valMax,$row->$postType);
		$load=true;
	}	
	return $load;
  }
  public function saveToSQL() {
	  foreach ($fieldsList as $f) {
		  $f->save();
	  }
  }
}

class CustomField {
 
 public function __construct($name="",$value="",$type="",$compare="=",$valMin=false,$valMax=false,$postType="hp_listing") {
  $this->name=$name;	 
  $this->value=$value;	 
  $this->type=$type;	 
  $this->compare=$compare;	   
  $this->valMin=$valMin;	
  $this->valMax=$valMax;	
  $this->postType=$postType;  
 }
 public function outName() {
	 return "{$this->name}";
 }
 public function outField() {
	 return "{$this->name};{$this->value};{$this->type};{$this->compare}";
 }
 public function outSelectOptions() {
	$values=explode(";",$this->value);
	foreach ($values as $val) {
	 $out.="<option value='$val'>$val</option>";	
	}
	return $out;
 }
 public function outFieldNice() {
	 if ($this->compare=="=") {
		 //lets gen a nice selectbox
		return "<label>{$this->name}</label>
		<select name='".$this->name."' 
		data-group='majax-fields' 
		id='custField".urlencode($this->name)."' 
		class='js-example-templating' 
		multiple='multiple'>
		{$this->outSelectOptions()}
		</select>";
	 }
	 if ($this->compare==">") {
		return "<label for='custField".urlencode($this->name)."'>{$this->name}</label>
			<input name='".$this->name."' data-group='majax-fields' data-mslider='majax-slider-".urlencode($this->name)."' id='custField".urlencode($this->name)."'></input>		
			<div id='majax-slider-".urlencode($this->name)."'></div>"; 
	 }
	 return "<label>{$this->name}</label><input name='".$this->name."' data-group='majax-fields' id='custField".urlencode($this->name)."'></input>";
 }
 public function initValues() {
	global $wpdb;
		
	$query="SELECT DISTINCT(`meta_value`) AS val FROM ".$wpdb->prefix."postmeta AS pm 
	WHERE pm.meta_key like '{$this->name}'";
	
	foreach( $wpdb->get_results($query) as $key => $row) {
		// each column in your row will be accessible like this
		//echo "<br />".$row->name;		
		if ($n>0) $vals.=";";
		$vals.=$row->val;		
		$n++;
	}	
	
	$this->value=$vals;
	//echo "<br />".$query;
 }
 public function initValMin() {
	global $wpdb;
	
	$query="SELECT MIN(`meta_value`) AS min FROM ".$wpdb->prefix."postmeta AS pm, ".$wpdb->prefix."posts AS po 
	WHERE pm.meta_key like '{$this->name}' AND po.post_status = 'publish' 
	AND po.post_type = '{$this->postType}'";
	
	$query="SELECT MIN(`meta_value`) AS min FROM ".$wpdb->prefix."postmeta AS pm 
	WHERE pm.meta_key like '{$this->name}'";
	
	$min = $wpdb->get_var($query);	 
	$this->valMin=$min;
	//echo "<br />".$query;
 }
  public function initValMax() {
	global $wpdb;
	$query = "SELECT MAX(`meta_value`) AS max FROM ".$wpdb->prefix."postmeta AS pm, ".$wpdb->prefix."posts AS po 
	WHERE pm.meta_key like '{$this->name}' AND po.post_status = 'publish' 
	AND po.post_type = '{$this->postType}'";	

	$query = "SELECT MAX(`meta_value`) AS max FROM ".$wpdb->prefix."postmeta AS pm 
	WHERE pm.meta_key like '{$this->name}'";	
	
	$max = $wpdb->get_var($query);	 
	$this->valMax=$max;
	//echo "<br />".$query;
 }
 public function getValMin() {
   if ($this->valMin===false) $this->initValMin();
   return $this->valMin;
 }
 public function getValMax() {
   if ($this->valMax===false) $this->initValMax();
   return $this->valMax;
 }
 public function getValues() {
   if ($this->values=="") $this->initValues();
   return $this->value;
 }
 public function getFieldFilter() {
   /*return "array(
				'key'		=> '".$this->name."',
				'value'		=> '".$_POST[$this->name]."',
				'type'		=> '".$this->type."',
				'compare'	=> '".$this->compare."'
			)";	 
			*/	
			//$_POST[$this->name]=$this->name."test";
			$val=$_POST[$this->name];
			if ($val=="") {
			 return false;	
			}
			if (strpos($val,"|")>0) {
				//multiple values in select field
				$compare="IN";
				if ($this->type=="NUMERIC") $compare="BETWEEN";
				return array(
					'key'		=> $this->name,
					'value'		=> explode("|",$val),
					'type'		=> $this->type,
					'compare'	=> $compare
				);				
			}
			else {
				//single value
				return array(
					'key'		=> $this->name,
					'value'		=> $_POST[$this->name],
					'type'		=> $this->type,
					'compare'	=> $this->compare
				);
			}
 }
 public function save() {
   global $wpdb;
   $query = "DELETE FROM `".$wpdb->prefix."majax_fields` WHERE `name` like '{$this->name}';";   
   $result = $wpdb->get_results($query);	 
   
   $query = "INSERT INTO `".$wpdb->prefix."majax_fields` ( `name`, `value`, `type`, `compare`, `valMin`, `valMax`, `postType`) 
	VALUES ('{$this->name}', '{$this->value}', '{$this->type}', '{$this->compare}', '{$this->valMin}', '{$this->valMax}', '{$this->postType}');";   
   $result = $wpdb->get_results($query);	 
   return "<br />{$this->name} saved $query";
 }
}

