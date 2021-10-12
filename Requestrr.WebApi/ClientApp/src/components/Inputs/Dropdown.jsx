import React from "react";
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";

class Dropdown extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      selectedValues: [{ name: "", value: 838738 }]
    }

    this.onValueChange = this.onValueChange.bind(this);
  }

  componentDidUpdate() {
    var newSelectedValues = this.props.items.length > 0 ? this.props.items.filter(x => x.value === this.props.value).length > 0 ? this.props.items.filter(x => x.value === this.props.value) : [this.props.items[0]] : [];

    if (JSON.stringify(this.state.selectedValues) !== JSON.stringify(newSelectedValues)) {
      this.setState({
        selectedValues: newSelectedValues
      })
    }
  }

  onValueChange = value =>
    this.props.onChange(value);

  render() {
    return (
      <FormGroup className={this.props.className}>
        <label
          className="form-control-label">
          {this.props.name}
        </label>

        <Select
          placeholder=""
          className="dropdown"
          options={this.props.items}
          values={this.state.selectedValues}
          labelField="name"
          valueField="value"
          searchable={false}
          clearable={false}
          onChange={(value) => {
            if (typeof (value) !== "undefined" && value.length > 0) {
              this.onValueChange(value[0].value);
            }
          }}
        />
      </FormGroup>
    );
  }
}

export default Dropdown;