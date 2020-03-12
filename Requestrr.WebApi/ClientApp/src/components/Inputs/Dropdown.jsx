import React from "react";
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";

class Dropdown extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      value: "",
    };

    this.onValueChange = this.onValueChange.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
  }

  componentDidMount() {
    this.updateStateFromProps(this.props);
  }

  componentWillReceiveProps(nextProps) {
    this.updateStateFromProps(nextProps);
  }

  updateStateFromProps = props => {
    if (this.state.value != props.value) {
      this.setState({ value: props.value });
    }
  };

  onValueChange = value => {
    this.setState({
      value: value,
    }, () => this.props.onChange(this.state.value));
  }

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
          values={this.props.items.filter(x => x.value === this.state.value)}
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