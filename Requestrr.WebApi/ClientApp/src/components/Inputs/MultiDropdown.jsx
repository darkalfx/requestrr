import React from "react";
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";

class MultiDropdown extends React.Component {
  constructor(props) {
    super(props);

    this.onSelectedItemsChange = this.onSelectedItemsChange.bind(this);
  }

  onSelectedItemsChange = selectedItems => {
    this.props.onChange(selectedItems);
  }

  render() {
    return (
      <FormGroup className={this.props.className}>
        <label
          className="form-control-label">
          {this.props.name}
        </label>
        <Select
          multi="true"
          className={this.props.create === true ? "dropdown react-dropdown-create" : "dropdown"}
          options={this.props.items}
          placeholder={this.props.placeholder}
          values={this.props.items.length > 0 ? this.props.items.filter(x => this.props.selectedItems.map(s => s.id).includes(x.id)).length > 0 ? this.props.items.filter(x => this.props.selectedItems.map(x => x.id).includes(x.id)) : [] : []}
          labelField="name"
          valueField="id"
          dropdownHandle={this.props.dropdownHandle !== false}
          searchable={this.props.searchable === true}
          create={this.props.create === true}
          clearable={false}
          color="#5e72e4"
          onChange={(values) => this.onSelectedItemsChange(values)}
        />
      </FormGroup>
    );
  }
}

export default MultiDropdown;