import React from "react";
import { Alert } from "reactstrap";

import {
  FormGroup,
  Input,
} from "reactstrap";

class ValidatedTextbox extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      value: "",
      hasValueChanged: false,
      isValueValid: true,
    };

    this.onValueChange = this.onValueChange.bind(this);
    this.triggerValueValidation = this.triggerValueValidation.bind(this);
  }

  componentDidMount() {
    if (this.state.value !== this.props.value) {
      this.setState({ value: this.props.value },
        () => this.triggerValueValidation());
    }
  }

  componentWillReceiveProps(nextProps) {
    if (this.state.value !== nextProps.value) {
      this.setState({ value: nextProps.value },
        () => this.triggerValueValidation());
    }

    if (this.props.isSubmitted !== nextProps.isSubmitted) {
      this.triggerValueValidation();
    }
  }

  onValueChange = event => {
    this.setState({
      value: event.target.value,
      hasValueChanged: true
    }, () => {
      this.triggerValueValidation();
      this.props.onChange(this.state.value);
    });
  }

  triggerValueValidation() {
    this.setState({
      isValueValid: this.props.validation(this.state.value)
    }, () => this.props.onValidate(this.state.isValueValid));
  }

  render() {
    return (
      <>
        <FormGroup className={!this.state.isValueValid ? this.props.className + " has-danger" : this.state.hasValueChanged ? this.props.className + " has-success" : this.props.className}>
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
          {
            !this.state.isValueValid ? (
              <Alert className={this.props.alertClassName} color="warning">
                <strong>{this.props.errorMessage}</strong>
              </Alert>)
              : null
          }
        </FormGroup>
      </>
    );
  }
}

export default ValidatedTextbox;