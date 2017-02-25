﻿// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Basic account management GUI.  Allows you to add
  // an email to your anonymous account, or to log out.
  class ManageAccount : BaseState {

    private Menus.ManageAccountGUI menuComponent;

    Firebase.Auth.FirebaseAuth auth;

    public override void Resume(StateExitValue results) {
      InitializeUI();

      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            manager.PushState(new BasicDialog("Could not sign in:\n" +
                taskResults.task.Exception.InnerException.ToString()));
          }
        }
      }
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      InitializeUI();
    }

    private void InitializeUI() {
      menuComponent = SpawnUI<Menus.ManageAccountGUI>(StringConstants.PrefabsManageAccountMenu);
      menuComponent.gameObject.SetActive(true);

      bool isAnon = auth.CurrentUser == null || auth.CurrentUser.IsAnonymous;
      menuComponent.SignedIntoEmailText.gameObject.SetActive(!isAnon);
      menuComponent.EmailText.gameObject.SetActive(!isAnon);
      menuComponent.SignedIntoAnonText.gameObject.SetActive(isAnon);
      menuComponent.AddEmailButton.gameObject.SetActive(isAnon);

      if (!isAnon) {
        string text;
        if (!string.IsNullOrEmpty(auth.CurrentUser.DisplayName)) {
          text = auth.CurrentUser.DisplayName + '\n';
        } else {
          text = StringConstants.UploadScoreDefaultName + '\n';
        }
        text += auth.CurrentUser.Email;
        menuComponent.EmailText.text = text;
      }
    }

    public override void Suspend() {
      menuComponent.gameObject.SetActive(false);
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(ManageAccount));
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.AddEmailButton.gameObject) {
        manager.PushState(new AddEmail());
      } else if (source == menuComponent.SignOutButton.gameObject) {
        auth.SignOut();
        // TODO(ccornell): if anonymous, clear out their user data
        // and any maps they've saved, so our DB doesn't get orphans.
        manager.ClearStack(new Startup());
      } else if (source == menuComponent.MainButton.gameObject) {
        manager.PopState();
      }
    }
  }
}
