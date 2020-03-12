import React from "react";
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";

class MultiDropdown extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      selectedItems: [],
    };

    this.onSelectedItemsChange = this.onSelectedItemsChange.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
  }

  componentDidMount() {
    this.updateStateFromProps(this.props);
  }

  componentWillReceiveProps(nextProps) {
    this.updateStateFromProps(nextProps);
  }

  updateStateFromProps = props => {
    if (props.selectedItems.length != this.state.selectedItems.length || props.selectedItems.filter(x => !this.state.selectedItems.includes(x)).length > 0) {
      this.setState({ selectedItems: props.selectedItems });
    }
  };

  onSelectedItemsChange = selectedItems => {
    this.setState({
      selectedItems: selectedItems,
    }, () => this.props.onChange(this.state.selectedItems));
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
          placeholder=""
          className="dropdown dropdown-multi"
          options={this.props.items}
          values={this.state.selectedItems}
          labelField={this.props.labelField}
          valueField={this.props.valueField}
          searchable={false}
          clearable={false}
          color="#5e72e4"
          onChange={(values) => this.onSelectedItemsChange(values)}
        />
      </FormGroup>
    );
  }
}

export default MultiDropdown;