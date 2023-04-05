
import React from "react";
import { useDispatch, useSelector } from 'react-redux';
import { addOverseerrTvShowCategory as addOverseerrCategory } from "../../../../store/actions/OverseerrClientSonarrActions"
import OverseerrTvShowCategory from "./OverseerrTvShowCategory";

// reactstrap components
import {
  FormGroup
} from "reactstrap";


function OverseerrTvShowCategoryList(props) {

  const reduxState = useSelector((state) => {
    return {
      overseerr: state.tvShows.overseerr
    }
  });
  const dispatch = useDispatch();


  const createOverseerrCategory = () => {
    let newId = Math.floor((Math.random() * 900) + 1);

    while (reduxState.overseerr.categories.map(x => x.id).includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1);
    }

    let newCategory = {
      id: newId,
      name: "new-category",
      serviceId: reduxState.overseerr.sonarrServiceSettings.sonarrServices.length > 0 ? reduxState.overseerr.sonarrServiceSettings.sonarrServices[0].id : -1,
      profileId: -1,
      rootFolder: "",
      tags: [],
      wasCreated: true
    };

    dispatch(addOverseerrCategory(newCategory));
  };

  

  return (
    <>
      <hr className="my-4" />
      <h6 className="heading-small text-muted">
        Overseerr Category Settings
      </h6>
      <div class="table-responsive mt-4 overflow-visible">
        <div>
          <table class="table align-items-center">
            <thead class="thead-dark">
              <tr>
                <th scope="col" class="sort" data-sort="name">Category</th>
                <th scope="col" class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="list">
              {reduxState.overseerr.categories.map((category, key) => {
                return (
                  <React.Fragment key={category.id}>
                    <OverseerrTvShowCategory key={category.id} isSubmitted={props.isSubmitted} isSaving={props.isSaving} canConnect={props.canConnect} apiVersion={props.apiVersion} category={{ ...category }} />
                  </React.Fragment>)
              })}
              <tr>
                <td className="text-right" colSpan="2">
                  <FormGroup className="form-group text-right mt-2">
                    <button onClick={createOverseerrCategory} className="btn btn-icon btn-3 btn-success" type="button">
                      <span className="btn-inner--icon"><i className="fas fa-plus"></i></span>
                      <span className="btn-inner--text">Add new category</span>
                    </button>
                  </FormGroup>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <hr className="my-4" />
    </>
  );
}

export default OverseerrTvShowCategoryList;