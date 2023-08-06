

import { useEffect, useState } from "react";
import { Alert } from "reactstrap";

import {
  FormGroup,
  Input,
} from "reactstrap";

function ValidatedTextbox(props) {
  const [value, setValue] = useState("");
  const [hasValueChanged, setHasValueChanged] = useState(false);
  const [isValueValid, setIsValueValid] = useState(true);


  useEffect(() => {
    if(props.value !== "")
      setValue(props.value);
  }, [props.value]);

  useEffect(() => {
    if(value !== "")
      triggerValueValidation();
  }, [value]);

  useEffect(() => {
    if (props.isSubmitted)
      triggerValueValidation();
  }, [props.isSubmitted]);
  


  const onValueChange = (event) => {
    setValue(event.target.value);
    setHasValueChanged(true);
    setIsValueValid(props.validation(event.target.value));
    props.onChange(event.target.value);
  };

  const triggerValueValidation = () => {
    const check = props.validation(value);
    setIsValueValid(check);
    props.onValidate(check);
  };



  return (
    <>
      <FormGroup className={!isValueValid ? props.className + " has-danger" : hasValueChanged ? props.className + " has-success" : props.className}>
        <label
          className="form-control-label"
          htmlFor={"input-" + props.name}>
          {props.name}
        </label>
        <Input
          value={value} onChange={onValueChange}
          className="form-control-alternative"
          id={"input-" + props.name}
          placeholder={props.placeholder}
          type="text" />
        {
          !isValueValid ? (
            <Alert className={props.alertClassName} color="warning">
              <strong>{props.errorMessage}</strong>
            </Alert>)
            : null
        }
      </FormGroup>
    </>
  );
}

export default ValidatedTextbox;