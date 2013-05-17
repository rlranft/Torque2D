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

#ifndef _CONSOLEBASECALLBACK_H_
#define _CONSOLEBASECALLBACK_H_

#ifndef _TORQUE_TYPES_H_
#include "platform/types.h"
#endif

class SimObject;

/// defines a callback category.  there should be one subclass per callback per object.
/// for instance, ScriptObject would have one for onAdd and one for onRemove.
class ConsoleBaseCallback
{
};

// base class for hoding "marshalled" data for  callback.
// for instance, a collision callback may have a subclas of this base
// which holds the object collided with, contact points, etc.
class ConsoleBaseCallbackData
{
public:
	virtual ~ConsoleBaseCallbackData() {}
};

/// callback coders should re-use NoCallbackData to mean
/// "no extra data for this callback"
class VoidCallbackData : public ConsoleBaseCallbackData
{
};

/// callback coders can re-use this single NoCallbackData instance to save space/time.
extern VoidCallbackData voidCallbackData;

class ConsoleObject;

typedef void (*ConsoleCallbackFunc)(SimObject* object, const char* callbackName, const ConsoleBaseCallbackData* data);

class AbstractConsoleCallbackConstructor
{
protected:

   static AbstractConsoleCallbackConstructor* first;
   AbstractConsoleCallbackConstructor* next;

   StringTableEntry mCallbackName;
   ConsoleCallbackFunc mCallbackFunc;

public:

   /// after the console has defined all callbacks it wants (globally during executable init),
   /// Con::init will call this to actually connect them, sometime after the console-based classes
   /// have had a chance to define them officially.  Here is where a run-time error will occur
   /// if a callback is desired from a class but that class does not define it.
   static void connectAllCallbacks();

   AbstractConsoleCallbackConstructor(const char *callbackName, ConsoleCallbackFunc func);

   virtual bool connectCallback() = 0;
};

/// This is the backend for the ConsoleCallback() macro.
///
/// @see ConsoleConstructor
template <class T>
class ConsoleCallbackConstructor : public AbstractConsoleCallbackConstructor
{
public:

   /// @name ConsoleCallbackConstructor
   ///
   /// The ConsoleCallback macro wrap the declaration of a ConsoleCallbackConstructor.
   /// The constructor holds data about a class.
   ///
   /// Because it is defined as a global, the constructor for the ConsoleConstructor is called
   /// before execution of main() is started. The constructor is called once for each global
   /// ConsoleConstructor variable, in the order in which they were defined (this property only holds true
   /// within file scope).
   ///
   /// We have ConsoleConstructor create a linked list at constructor time, by storing a static
   /// pointer to the head of the list, and keeping a pointer to the next item in each instance
   /// of ConsoleConstructor. init() is a helper function in this process, automatically filling
   /// in commonly used fields and updating first and next as needed. In this way, a list of
   /// items to add to the console is assemble in memory, ready for use, before we start
   /// execution of the program proper.
   ///
   /// In Con::init(), ConsoleConstructor::setup() is called to process this prepared list. Each
   /// item in the list is iterated over, and the appropriate Con namespace functions (usually
   /// Con::addCommand) are invoked to register the ConsoleFunctions and ConsoleMethods in
   /// the appropriate namespaces.
   ///

   ConsoleCallbackConstructor(const char *callbackName, ConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func) {}

	bool connectCallback() {
		AbstractClassRep* classRep = T::getStaticClassRep();

		Vector<AbstractClassRep::CallbackType>& callbackTypes = classRep->mCallbackTypes;
		const AbstractClassRep::CallbackType* callbackType = NULL;
		for (S32 i = 0; i < callbackTypes.size(); i++) {
			if (callbackTypes[i].mName == mCallbackName) {
				callbackType = &callbackTypes[i];
				break;
			}
		}

		if (callbackType == NULL) {
			// warn no callback of this type defined
			return false;
		}

		AbstractClassRep::CallbackEntry cbe;
		cbe.mType = callbackType;
		cbe.mFunc = mCallbackFunc;
		classRep->mCallbacks.push_back(cbe);
		return true;
	}
};

#endif // _CONSOLEBASECALLBACK_H_
