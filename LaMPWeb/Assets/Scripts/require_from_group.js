/*
* Lets you say "at least X inputs that match selector Y must be filled."
*
* The end result is that neither of these inputs:
*
*	<input class="productinfo" name="partnumber">
*	<input class="productinfo" name="description">
*
*	...will validate unless at least one of them is filled.
*
* partnumber:	{require_from_group: [1,".productinfo"]},
* description: {require_from_group: [1,".productinfo"]}
*
* options[0]: number of fields that must be filled in the group
* options[1]: CSS selector that defines the group of conditionally required fields
*/
$.validator.addMethod("require_from_group", function(value, element, options) {
  var numberRequired = options[0];
  var selector = options[1];
  var fields = $(selector, element.form);
  var filled_fields = fields.filter(function() {
    // it's more clear to compare with empty string
    return $(this).val() != ""; 
  });
  var empty_fields = fields.not(filled_fields);
  // we will mark only first empty field as invalid
  if (filled_fields.length < numberRequired && empty_fields[0] == element) {
    return false;
  }
  return true;
// {0} below is the 0th item in the options field
}, jQuery.format("Please fill out at least {0} of these fields."));