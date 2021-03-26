<?php
   /*
   Plugin Name: Majax plugin
   Plugin URI: http://ttj.cz
   description: >-
  majax plugin
   Version: 1.2
   Author: Mik
   Author URI: http://ttj.cz
   License: GPL2
   */
register_activation_hook( __FILE__, 'majax_plugin_install' );
function majax_plugin_install() {
	global $wpdb;	
	echo "installing..";
	$table_name = $wpdb->prefix . "majax_fields"; 	
	$charset_collate = $wpdb->get_charset_collate();
	
	/*
	$query = "TRUNCATE TABLE `$table_name`";   	
	$result = mysqli_query($wpdb->dbh,$query);
	*/
	$query = "DROP TABLE `$table_name`";   	
	$result = mysqli_query($wpdb->dbh,$query);
	
	$sql = "CREATE TABLE $table_name (
	  id mediumint(9) NOT NULL AUTO_INCREMENT,	
	  name tinytext,
	  value text,
	  type tinytext,
	  compare tinytext,
	  valMin text,
	  valMax text,
	  postType tinytext,
	  PRIMARY KEY  (id)
	) $charset_collate;";
	

	require_once( ABSPATH . 'wp-admin/includes/upgrade.php' );
	dbDelta( $sql );
	
	$welcome_name = 'Mr. WordPress';
	$welcome_text = 'Congratulations, you just completed the installation!';


	$wpdb->insert( 
		$table_name, 
		array( 
			'time' => current_time( 'mysql' ), 
			'name' => $welcome_name, 
			'text' => $welcome_text, 
		) 
	);

}


$majax=new Majax();
   
Class Majax {
	
	function __construct() {
		include_once "custom-field.php";
		//init custom fields
		$this->fields=new CustomFields();
		
		//kdyz uz mam nacteny pole
		if (!$this->fields->loadFromSQL()) {
			//manual setup
			//kdyz jeste ne		
			$this->fields->addField(new CustomField("hp_price",10,"NUMERIC",">"));
			$this->fields->addField(new CustomField("hp_vendor","Milanoo","","="));		
			echo $this->fields->initFields();
		}			
		
		//init actions
		add_action( 'wp_enqueue_scripts', [$this,'mAjaxEnqueue'] );		
		add_action('wp_ajax_filter_projects', [$this,'filter_projects_continuous'] );
		add_action('wp_ajax_nopriv_filter_projects', [$this,'filter_projects_continuous'] );

		//add shortcode
		add_shortcode('majax', [$this,'majax_print_filter'] );

		//add style
		wp_register_style( 'majax', plugin_dir_url( __FILE__ ) . 'majax.css' );
		wp_enqueue_style('majax');
		
		//add select2
		wp_register_style( 'select2', 'https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/css/select2.min.css' );
		wp_enqueue_style('select2');
		wp_enqueue_script( 'select2', 'https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/js/select2.min.js' );
		
		
		//add jquery
		/*	
		wp_enqueue_script( 'jquery3-5-1', 'https://code.jquery.com/jquery-3.3.1.slim.min.js' );
		
		//add bootstrap
		wp_register_style( 'bootstrap-css', 'https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta/dist/css/bootstrap-select.min.css' );
		wp_enqueue_style('bootstrap-css');
		
		wp_enqueue_script( 'popperjs', 'https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.min.js' );				
		wp_enqueue_script( 'bootstrap-js', 'https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js' );
		wp_enqueue_script( 'bootstrap-select', 'https://cdn.jsdelivr.net/npm/bootstrap-select@1.14.0-beta/dist/js/bootstrap-select.min.js' );
		*/
	}

	function mAjaxEnqueue() {
		//vlozi js
		wp_enqueue_script( 'ajax-script', plugin_dir_url( __FILE__ ) . 'majax.js', array('jquery') );
		wp_localize_script( 'ajax-script', 'majax',
				array( 'ajax_url' => admin_url( 'admin-ajax.php' ) ) );
	}
	function initFields() {
		echo $this->fields->initFields();
	}
	function majax_print_filter() {	
		$this->logWrite("log mik");	
		//echo "fields:".$this->fields->outFields();					
		echo "fields:";
		foreach ($this->fields->getList() as $fields) {
		  echo $fields->outFieldNice();	
		}
		//$query=$this->buildQuery();
		//print_r($query);
		
		//vypsani filterovaciho boxu
		$categories = get_terms(array('taxonomy' => $taxonomy,'hide_empty' => false,'parent' => $thisTerm->term_id, 'depth' => 1,'number' => 5));	
		ob_start();
		?>

		<ul class="cat-list">
		  <li><a class="cat-list_item active" href="#!" data-slug="">All projects</a></li>

		  <?php foreach($categories as $category) : 
			$n++;
			?>
			<li>
			  <a class="cat-list_item" href="#!" data-slug="<?= $category->slug; ?>" data-type="mik<?=$n?>">
				<?= $category->name; ?>
			  </a>
			</li>
		  <?php endforeach; ?>
		</ul>  
		
		<ul class="project-tiles">
		 <?php
		  //ajax content comes here
		 ?>
		</ul> <?php
		 return ob_get_clean();
	}	
	function buildQuery() {
			 /*
	 $ajaxposts = new WP_Query([
		'post_type' => 'hp_listing',
		'posts_per_page' => 8,
		'orderby' => 'menu_order', 
		'order' => 'desc',
		'taxonomy' => 'hp_listing_category',
		'taxonomy_terms' => $catSlug,
		'meta_query'	=> array(
			'relation'		=> 'AND',
			array(
				'key'		=> 'hp_vendor',
				'value'		=> 'Milanoo',
				'compare'	=> '='
			),
			array(
				'key'		=> 'hp_price',
				'value'		=> 10,
				'type'		=> 'NUMERIC',
				'compare'	=> '>'
			)
		)
	  ]); 
	  */
	  
	  $catSlug = $_POST['category'];
	  $mType = $_POST['type'];	
	  
	  $postType="hp_listing";
	  $taxonomy="hp_listing_category";
	  //nacteni filtrovanych custom_fields
	  //$metaQuery=array();
	  $metaQuery["relation"] = 'AND';
	  /*
	  $this->logWrite("action: ".$_POST['action']);
	  $this->logWrite("type: ".$_POST['type']);
	  $this->logWrite("category: ".$_POST['category']);
	  */
	  foreach ($this->fields->getList() as $field) {
		  $filter = $field->getFieldFilter();	
		  if ($filter) { 		
		    $metaQuery[] = $filter;
		    $this->logWrite("name: {$field->name} filter: ".$filter." - ".$_POST[$field->name]);   		   
		  } 
	  }
	  $wpQuery=[
		'post_type' => $postType,
		'posts_per_page' => 8,
		'orderby' => 'menu_order', 
		'order' => 'desc',
		'taxonomy' => $taxonomy,
		'taxonomy_terms' => $catSlug,	
	  ];
	  $wpQuery["meta_query"]=$metaQuery;  
	  $this->logWrite("query: ".json_encode($wpQuery));
	  return $wpQuery;
	}
	function filter_projects_continuous() {
	  //tohle natahuje data pro ajax jeden post po jednom, vraci json
	  
	  /*
	  $customFields=explode(";","hp_price;hp_vendor");
	  
	  'meta_query'	=> array(
			'relation'		=> 'AND',
			array(
				'key'		=> 'hp_vendor',
				'value'		=> 'Milanoo',
				'compare'	=> '='
			),
			array(
				'key'		=> 'hp_price',
				'value'		=> 10,
				'type'		=> 'NUMERIC',
				'compare'	=> '>'
			)
		)
	  */
	    
	  $ajaxposts = new WP_Query($this->buildQuery());
		  
	  if($ajaxposts->have_posts()) {
		while($ajaxposts->have_posts()) : 
		  $ajaxposts->the_post();	  
		  $ajaxPost->title=get_the_title();
		  $ajaxPost->content=get_the_title();
		  $ajaxPost->url=get_the_permalink();	
		  $ajaxPost->meta=get_post_meta(get_the_id(),"hp_vendor",true);	
		  echo json_encode($ajaxPost);
		  flush();
		  ob_flush();
		  usleep(500000);
		endwhile;
		exit;
	  } else {	
		$response="";
		echo json_encode($response);
	    exit;
	  }
	}
	
	function logWrite($val) {
	 file_put_contents(plugin_dir_path( __FILE__ ) . "log.txt",date("d-m-Y h:i:s")." ".$val."\n",FILE_APPEND | LOCK_EX);
	}

	// -> obsolete
	function filter_projects() {
	  //tohle natahuje data pro ajax vsechny posty najednou
	  $catSlug = $_POST['category'];
	  $mType = $_POST['type'];
	  //$catSlug = "hp_listing_category";
	  
	  $ajaxposts = new WP_Query([
		'post_type' => 'hp_listing',
		'posts_per_page' => 8,
	//	'category_name' => $catSlug,
		'orderby' => 'menu_order', 
		'order' => 'desc',
		'taxonomy' => 'hp_listing_category',
		'taxonomy_terms' => $catSlug,	
	  ]);
	  
		
	  $response = "---".$catSlug."---";

	  if($ajaxposts->have_posts()) {
		while($ajaxposts->have_posts()) : $ajaxposts->the_post();
		  $response .= get_template_part('template-list-item');
		  //$response = $response . the_title();
		  $response .= get_the_title();
		  $response .= "<b>".$mType."</b>";
		endwhile;
	  } else {
		$response .= 'empty';
	  }

	  echo $response;
	  exit;
	}

	function filter_projects_continuous_html() {
	  //tohle natahuje data pro ajax jeden post po jednom, vraci html
	  $catSlug = $_POST['category'];
	  $mType = $_POST['type'];
	  //$catSlug = "hp_listing_category";
	  
	  $ajaxposts = new WP_Query([
		'post_type' => 'hp_listing',
		'posts_per_page' => 8,
	//	'category_name' => $catSlug,
		'orderby' => 'menu_order', 
		'order' => 'desc',
		'taxonomy' => 'hp_listing_category',
		'taxonomy_terms' => $catSlug,	
	  ]);
	  
	  //query s custom fields
	 /*
	 $ajaxposts = new WP_Query([
		'post_type' => 'hp_listing',
		'posts_per_page' => 8,
		'orderby' => 'menu_order', 
		'order' => 'desc',
		'taxonomy' => 'hp_listing_category',
		'taxonomy_terms' => $catSlug,
		'meta_query'	=> array(
			'relation'		=> 'AND',
			array(
				'key'		=> 'hp_vendor',
				'value'		=> 'Milanoo',
				'compare'	=> '='
			),
			array(
				'key'		=> 'hp_price',
				'value'		=> 10,
				'type'		=> 'NUMERIC',
				'compare'	=> '>'
			)
		)
	  ]); 
	  */
		  
	  if($ajaxposts->have_posts()) {
		while($ajaxposts->have_posts()) : $ajaxposts->the_post();
		  //$response .= get_template_part('template-list-item');
		  //$response = $response . the_title();
		  //$part_response = get_the_content();	  
		  $part_response = get_template_part('wpb-single-post');	  
		  echo $part_response;
		  flush();
		  ob_flush();
		  usleep(500000);
		endwhile;
		exit;
	  } else {
		$response .= 'empty';
	  }

	  echo $response;
	  exit;
	}

}
?>