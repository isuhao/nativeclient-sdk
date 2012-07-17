#!/usr/bin/env python
# Copyright (c) 2012 The Chromium Authors. All rights reserved.
# Use of this source code is governed by a BSD-style license that can be
# found in the LICENSE file.

"""Copies necessary add-in files into place to install the add-in.

This script will copy the necessary files for the Visual Studio add-in
to where Visual Studio can find them. It assumes the current directory
contains the necessary files to copy.
"""

import os
import platform
import shutil

def main():
  if platform.system() != 'Windows':
    raise Exception('Must install to Windows system')

  # Ensure environment variables are set
  nacl_sdk_root = os.path.expandvars('%NACL_SDK_ROOT%', None)
  chrome_path = os.path.expandvars('%CHROME_PATH%', None)
  if nacl_sdk_root == None:
    raise Exception('Environment Variable NACL_SDK_ROOT is not set')
  if chrome_path == None:
    raise Exception('Environment Variable CHROME_PATH is not set')

  # Copy the necessary files into place
  addInDir = os.path.expandvars(
    '%USERPROFILE%\My Documents\Visual Studio 2010\Addins')
  shutil.copy('./NativeClientVSAddIn.AddIn', addInDir)
  shutil.copy('./NativeClientVSAddIn.dll', addInDir)

if __name__ == '__main__':
  main()
