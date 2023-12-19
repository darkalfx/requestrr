
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";


function MultiDropdown(props) {

  const onSelectedItemsChange = (selectedItems) => {
    props.onChange(selectedItems);
  }

  return (
    <FormGroup className={props.className}>
      <label
        className="form-control-label">
        {props.name}
      </label>
      <Select
        multi="true"
        className={props.create === true ? "dropdown react-dropdown-create" : "dropdown"}
        options={props.items}
        placeholder={props.placeholder}
        values={props.items.length > 0 ? props.items.filter(x => props.selectedItems.map(s => s.id).includes(x.id)).length > 0 ? props.items.filter(x => props.selectedItems.map(x => x.id).includes(x.id)) : [] : []}
        labelField="name"
        valueField="id"
        dropdownHandle={props.dropdownHandle !== false}
        searchable={props.searchable === true}
        create={props.create === true}
        clearable={false}
        color="#5e72e4"
        onChange={(values) => onSelectedItemsChange(values)}
      />
    </FormGroup>
  );
}

export default MultiDropdown;