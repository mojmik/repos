<?php
/*
 * Template Name: Mik cau template
 * Template Post Type: post, page, product
 */

  
 ?>
cau tempelejt1
<div class="row">
	<main class="col-sm-8 col-xs-12 <?php if ( ! is_active_sidebar( 'blog_sidebar' ) ) : ?>col-sm-offset-2<?php endif; ?>">
		<?php
		get_template_part( 'templates/post-archive' );		
		?>
	</main>  
</div>  
