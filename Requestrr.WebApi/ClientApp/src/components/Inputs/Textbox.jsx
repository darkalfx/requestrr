import React from "react";

import {
  FormGroup,
  Input,
} from "reactstrap";

class Textbox extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      value: "",
    };

    this.onValueChange = this.onValueChange.bind(this);
  }

  componentDidMount() {
    if (this.state.value !== this.props.value) {
      this.setState({ value: this.props.value });
    }
  }

  componentWillReceiveProps(nextProps) {
    if (this.state.value !== nextProps.value) {
      this.setState({ value: nextProps.value });
    }
  }

  onValueChange = event => {
    this.setState({
      value: event.target.value,
      hasValueChanged: true
    }, () => {
      this.props.onChange(this.state.value);
    });
  }

  render() {
    return (
      <>
        <FormGroup className={this.props.className}>
          <label
            className="form-control-label"
            htmlFor={"input-" + this.props.name}>
            {this.props.name}
          </label>
          <Input
            value={this.state.value} onChange={this.onValueChange}
            className="form-control-alternative"
            id={"input-" + this.props.name}
            placeholder={this.props.placeholder}
            type="text" />
        </FormGroup>
      </>
    );
  }
}

export default Textbox;