
import { useEffect, useState } from "react";
import {
  FormGroup,
  Input,
} from "reactstrap";


function TextboxTest(props) {
  const [value, setValue] = useState("");
  const [hasValueChanged, setHasValueChanged] = useState(false);

  

  useEffect(() => {
    if (value !== props.value)
      setValue(props.value);
  }, []);


  useEffect(() => {
    if (value !== props.value)
      setValue(props.value);
  }, [props]);


  useEffect(() => {
    props.onChange(value);
  }, [hasValueChanged]);




  const onValueChange = (event) => {
    setValue(event.target.value);
    setHasValueChanged(true);
  };



  return (
    <>
      <FormGroup className={props.className}>
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
      </FormGroup>
    </>
  );
}

export default TextboxTest;