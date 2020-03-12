/*!

=========================================================
* Argon Dashboard React - v1.0.0
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-react
* Copyright 2019 Creative Tim (https://www.creative-tim.com)
* Licensed under MIT (https://github.com/creativetimofficial/argon-dashboard-react/blob/master/LICENSE.md)

* Coded by Creative Tim

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

*/
import React from "react";
import { connect } from 'react-redux';
import { logout } from "../store/actions/UserActions"

class Logout extends React.Component {
  componentDidMount() {
    this.props.logout();
  };

  render() {
    return null;
  }
};

const mapPropsToAction = {
  logout: logout
};

export default connect(null, mapPropsToAction)(Logout);