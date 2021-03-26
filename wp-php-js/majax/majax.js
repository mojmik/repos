
const postTemplate = (id,title,content,url,meta) => `
<div class='ajaxout' id='ajaxout${id}'>
 title ${title} ${content} meta <b>${meta}</b>
</div>

`;

function getAjaxParams(varThisObj) {
 var thisId=0;
 var thisHtml="";
 var last_response_len = false;
  
 var outObj={
				type: 'POST',
				data: {
					  action: 'filter_projects',
					  category: varThisObj.data('slug'),
					  type: varThisObj.data('type')
				},
				beforeSend: function() {
				 jQuery('.project-tiles').empty();				 
				},
				xhrFields: {
					onprogress: function(e)
					{
						var this_response, response = e.currentTarget.response;
						var jsonObj;
						if(last_response_len === false)
						{
							this_response = response;
							last_response_len = response.length;
						}
						else
						{
							this_response = response.substring(last_response_len);
							last_response_len = response.length;
						}
						//console.log(this_response);
						thisId++;
						if (this_response!="") {
							jsonObj=JSON.parse(this_response);
							thisHtml=postTemplate(thisId,jsonObj.title,jsonObj.content,jsonObj.url,jsonObj.meta);
							jQuery('.project-tiles').append(thisHtml);
							jQuery("#ajaxout"+thisId).fadeIn("slow");
						}
					}
				}
 };			
 
 //inputs
 var inputFields = jQuery('input[data-group="majax-fields"]');
 inputFields.each(function (i,obj) {
	 let sliderId=jQuery(this).attr('data-mslider');	 
	 if (sliderId != "") {
		 //special slider input
		 outObj.data[obj.name]=formatSliderVal(obj.value);
	 }
	 else {		
		//ordinary input
		outObj.data[obj.name]=obj.value;
	 }
 });
 
 //selects
 var selects=jQuery('select[data-group="majax-fields"]');
 selects.each(function (i,obj) {
				//outObj.data[obj.name]=obj.value;				
				//rozbaleni dat pro vyfiltrovani
				var selectedText="";
				var selectData=jQuery(obj).select2('data');	
				var n=0;				
				jQuery.each(selectData, function (selIndex,selObj) {
					if (selObj.selected) { 
					 if (n>0) selectedText += "|";
					 selectedText += selObj.id;
					 n++;
					}
				});				
				outObj.data[obj.name]=selectedText;
 });
 return outObj;
}

function runAjax(firingElement) {
 var ajaxPar=getAjaxParams(jQuery(firingElement));	 
 jQuery.ajax(majax.ajax_url, ajaxPar)
			.done(function(dataOut)
			{
				//console.log('Complete response = ' + dataOut);
			})
			.fail(function(dataOut)
			{
				//console.log('Error: ', dataOut);
			});
			//console.log('Request Sent');
}

const continuousLoad=function() {								
		jQuery('.cat-list_item').on('click', function() {				
			runAjax(this);
		});
}

const selectAjax=function() {
	jQuery('.js-example-templating').on('change', function() {	
			runAjax(this);
		});
}

function formatState (state) {
  if (!state.id) {
    return state.text;
  }

  var baseUrl = "/user/pages/images/flags";
  var $state = jQuery(
    '<span><img class="img-flag" /> <span></span></span>'
  );

  // Use .text() instead of HTML string concatenation to avoid script injection issues
  $state.find("span").text(state.text);
  $state.find("img").attr("src", baseUrl + "/" + state.element.value.toLowerCase() + ".png");

  return $state;
};

jQuery(document).ready(function() {
	//fire event handlers
	continuousLoad();
	selectAjax();
	
	//select2
    jQuery(".js-example-templating").select2({
		templateResult: formatState,
		templateSelection: formatState
	});
	
	//sliders
	initSliders(); 
});

function formatSliderVal(val1,val2=0,dir=1) {
 const mask="$"	+ val1 + " - " + "$" + val2;	
 if (dir==0) return "$"	+ val1 + " - " + "$" + val2;
 
 //let's take 2 numbers
 const rex = /-?\d(?:[,\d]*\.\d+|[,\d]*)/g;
 let out="";
 while ((match = rex.exec(val1)) !== null) {
	if (out!="") out+='|'; 
    out+=match[0];
 }
 return out;
}

function initSliders() {
 //initialize numeric sliders
 jQuery('input[data-mslider]').each(function(index) {
	 var inputId=this.id;
	 let sliderId=jQuery(this).attr('data-mslider');
	 let sliderRange=jQuery('#'+sliderId+'');
	 jQuery(sliderRange).slider({
      range: true,
      min: 0,
      max: 500,
      values: [ 75, 300 ],
      slide: function( event, ui ) {
        jQuery('#'+inputId).val(formatSliderVal(ui.values[ 0 ],ui.values[ 1 ],0));
      }
    });
	sliderRange.on('slidestop',function(e) {
	  runAjax(this);
	});
    jQuery(this).val(formatSliderVal(jQuery(sliderRange).slider( "values", 0 ),jQuery(sliderRange).slider( "values", 1 ),0));
 });
}