//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

#include "console/consoleTypes.h"
#include "console/consoleBaseCallback.h"
#include "string/stringTable.h"

/// callback coders can re-use this single VoidCallbackData instance to save space/time.
VoidCallbackData voidCallbackData;

AbstractConsoleCallbackConstructor* AbstractConsoleCallbackConstructor::first = NULL;

AbstractConsoleCallbackConstructor::AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::StringConsoleCallbackFunc sfunc)
{
   mCallbackName = StringTable->insert(callbackName);
   mReturnType = AbstractClassRep::StringReturn;
   mCallbackFunction.stringConsoleCallbackFunc = sfunc;
   next = first;
   first = this;
}

AbstractConsoleCallbackConstructor::AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::IntConsoleCallbackFunc ifunc)
{
   mCallbackName = StringTable->insert(callbackName);
   mReturnType = AbstractClassRep::IntReturn;
   mCallbackFunction.intConsoleCallbackFunc = ifunc;
   next = first;
   first = this;
}

AbstractConsoleCallbackConstructor::AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::FloatConsoleCallbackFunc ffunc)
{
   mCallbackName = StringTable->insert(callbackName);
   mReturnType = AbstractClassRep::FloatReturn;
   mCallbackFunction.floatConsoleCallbackFunc = ffunc;
   next = first;
   first = this;
}

AbstractConsoleCallbackConstructor::AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::VoidConsoleCallbackFunc vfunc)
{
   mCallbackName = StringTable->insert(callbackName);
   mReturnType = AbstractClassRep::VoidReturn;
   mCallbackFunction.voidConsoleCallbackFunc = vfunc;
   next = first;
   first = this;
}

AbstractConsoleCallbackConstructor::AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::BoolConsoleCallbackFunc bfunc)
{
   mCallbackName = StringTable->insert(callbackName);
   mReturnType = AbstractClassRep::BoolReturn;
   mCallbackFunction.boolConsoleCallbackFunc = bfunc;
   next = first;
   first = this;
}

void AbstractConsoleCallbackConstructor::connectAllCallbacks()
{
   for(AbstractConsoleCallbackConstructor *walk = first; walk; walk = walk->next)
	   walk->connectCallback();
}
