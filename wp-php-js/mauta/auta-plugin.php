<?php
   /*
   Plugin Name: mAuta plugin
   Plugin URI: http://ttj.cz
   description: >-
  mAuta plugin
   Version: 1.2
   Author: Mik
   Author URI: http://ttj.cz
   License: GPL2
   */
   
register_activation_hook( __FILE__, 'auta_plugin_install' );

function auta_plugin_install() {
	global $wpdb;	
	echo "installing..";
	$table_name = $wpdb->prefix . "auta"; 
	
	$charset_collate = $wpdb->get_charset_collate();

	$sql = "CREATE TABLE $table_name (
	  id mediumint(9) NOT NULL AUTO_INCREMENT,
	  time datetime DEFAULT '0000-00-00 00:00:00' NOT NULL,
	  name tinytext NOT NULL,
	  text text NOT NULL,
	  url varchar(55) DEFAULT '' NOT NULL,
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

$autaCPT=new AutaCustomPost(); 

class AutaCustomPost {

	 public function __construct() {
		 /* Hook into the 'init' action so that the function
		  * Containing our post type registration is not 
	      * unnecessarily executed. 
		 */ 
		 add_action( 'init', [$this,'custom_post_type'] , 0 );
		 
		 //admin
		 add_action('admin_menu' , [$this,'add_to_admin_menu']); 
		 
		 //init custom fields
		 $this->init_custom_fields();
	 }
	 	
	/*
	* Creating a function to create our CPT
	*/
	function custom_post_type() {
	 $textDomain="auta-cpt"; //for If your theme is translation ready, and you want your custom post types to be translated, then you will need to mention text domain used by your theme.
	// Set UI labels for Custom Post Type
		$labels = array(
			'name'                => _x( 'Auta', 'Post Type General Name', $textDomain ),
			'singular_name'       => _x( 'Auto', 'Post Type Singular Name', $textDomain ),
			'menu_name'           => __( 'Auta', $textDomain ),
			'parent_item_colon'   => __( 'Nadřazené auto', $textDomain ),
			'all_items'           => __( 'Všechna auta', $textDomain ),
			'view_item'           => __( 'Zobrazit auto', $textDomain ),
			'add_new_item'        => __( 'Přidat auto', $textDomain ),
			'add_new'             => __( 'Přidat nové', $textDomain ),
			'edit_item'           => __( 'Upravovat auto', $textDomain ),
			'update_item'         => __( 'Aktualizovat auto', $textDomain ),
			'search_items'        => __( 'Hledat auto', $textDomain ),
			'not_found'           => __( 'Nenalezeno', $textDomain ),
			'not_found_in_trash'  => __( 'Nenalezeno v koši', $textDomain ),
		);
		 
	// Set other options for Custom Post Type
		 
		$args = array(
			'label'               => __( 'auta', $textDomain ),
			'description'         => __( 'Auta v nabídce', $textDomain ),
			'labels'              => $labels,
			// Features this CPT supports in Post Editor
			'supports'            => array( 'title', 'editor', 'excerpt', 'author', 'thumbnail', 'comments', 'revisions', 'custom-fields', ),
			// You can associate this CPT with a taxonomy or custom taxonomy. 
			'taxonomies'          => array( 'skupiny' ),
			/* A hierarchical CPT is like Pages and can have
			* Parent and child items. A non-hierarchical CPT
			* is like Posts.
			*/ 
			'hierarchical'        => false,
			'public'              => true,
			'show_ui'             => true,
			'show_in_menu'        => true,
			'show_in_nav_menus'   => true,
			'show_in_admin_bar'   => true,
			'menu_position'       => 5,
			'can_export'          => true,
			'has_archive'         => true,
			'exclude_from_search' => false,
			'publicly_queryable'  => true,
			'capability_type'     => 'post',
			'show_in_rest' => true,
	 
		);
		 
		// Registering your Custom Post Type
		register_post_type( 'mauta', $args );
	 
	 }
	 
	function auta_menu_function() {
		echo 'ahoj auta';	
		return "neco";
	}

	 function add_to_admin_menu() {
		//add_submenu_page( string $parent_slug, string $page_title, string $menu_title, string $capability, string $menu_slug, callable $function = '', int $position = null )
		$parent_slug='edit.php?post_type=mauta';
		$page_title='Auta admin';
		$menu_title='Auta';
		$capability='edit_posts';
		$menu_slug=basename(__FILE__);
		$function = [$this,'auta_menu_function'];
		add_submenu_page($parent_slug, $page_title, $menu_title, $capability, $menu_slug, $function);
	} 
	
	function init_custom_fields() {
		$this->autaFields = new AutaFields(); 
		$this->autaFields->fieldsList[] = new AutaField("razeni","string");
	}
}

class AutaFields {
	public $fieldsList=array();
	public function __construct() {
		add_action( 'add_meta_boxes_mauta', [$this,'mauta_metaboxes'] );		
		add_action( 'save_post_mauta', [$this,'mauta_save_post'] ); 
	}
	function mauta_metaboxes( ) {
		global $wp_meta_boxes;
		add_meta_box('postfunctiondiv', __('Razeni'), [$this,'mauta_metaboxes_html'], 'mauta', 'normal', 'high');
		//add_meta_box('postfunctiondiv', __('Pohon'), [$this,'mauta_metaboxes_html_pohon'], 'mauta', 'normal', 'high');
	}
	function mauta_metaboxes_html() 	{
		global $post;
		$custom = get_post_custom($post->ID);
		$razeni = isset($custom["razeni"][0])?$custom["razeni"][0]:'';
		$pohon = isset($custom["pohon"][0])?$custom["pohon"][0]:'';
	?>
		<label>Razeni:</label><input name="razeni" value="<?php echo $razeni; ?>">
		<label>Pohon:</label><input name="pohon" value="<?php echo $pohon; ?>">
	<?php
	}	
	function mauta_metaboxes_html_pohon() 	{
		global $post;
		$custom = get_post_custom($post->ID);
		$pohon = isset($custom["pohon"][0])?$custom["pohon"][0]:'';
	?>
		<label>pohon:</label><input name="pohon" value="<?php echo $pohon; ?>">
	<?php
	}	
	function mauta_save_post()	{
		if(empty($_POST)) return; //tackle trigger by add new 
		global $post;
		update_post_meta($post->ID, "razeni", $_POST["razeni"]);
		update_post_meta($post->ID, "pohon", $_POST["pohon"]);
	}   
}

class AutaField {
 public function __construct($name="",$type="") {
	  $this->name=$name;	 
	  $this->type=$type;	 	  
 } 
}