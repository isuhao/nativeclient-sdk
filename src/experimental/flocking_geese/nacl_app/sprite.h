// Copyright (c) 2011 The Native Client Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#ifndef SPRITE_H_
#define SPRITE_H_

#include <vector>
#include "boost/scoped_ptr.hpp"
#include "boost/noncopyable.hpp"
#include "ppapi/cpp/point.h"
#include "ppapi/cpp/rect.h"
#include "ppapi/cpp/size.h"

namespace flocking_geese {

// A Sprite is a simple container of a pixel buffer.  It knows how to
// composite itself to another pixel buffer of the same format.
class Sprite : public boost::noncopyable {
 public:
  // Initialize a Sprite to use the attached pixel buffer.  The Sprite takes
  // ownership of the pixel buffer, deleting it in the dtor.  The pixel
  // buffer is assumed to be 32-bit ARGB-8-8-8-8 pixel format, with pre-
  // multiplied alpha.  If |row_bytes| is 0, then the number of bytes per row
  // is assumed to be size.width() * sizeof(uint32_t).
  Sprite(uint32_t* pixel_buffer, const pp::Size& size, int32_t row_bytes);

  // Delete the pixel buffer.  It is assumed that the pixel buffer was created
  // using malloc().
  ~Sprite() {}

  // Reset the internal pixel buffer to a new one.  Deletes the old pixel
  // buffer.  Sprite takes ownership of the new pixel buffer.  If |row_bytes|
  // is 0, then the number of bytes per row is assumed to be size.width() *
  // sizeof(uint32_t).
  void SetPixelBuffer(uint32_t* pixel_buffer,
                      const pp::Size& size,
                      int32_t row_bytes);

  // Composite the section of the Sprite contained in |src_rect| into the given
  // pixel buffer at |dest_point|.  Performs a "source-over" composite
  // operation, and all necessary clipping.  Assumes pre-mulitplied alpha.
  void CompositeFromRectToPoint(const pp::Rect& src_rect,
                                uint32_t* dest_pixel_buffer,
                                const pp::Size& dest_size,
                                int32_t dest_row_bytes,
                                const pp::Point& dest_point) const;

  // Accessors.
  const pp::Size& size() const {
    return pixel_buffer_size_;
  }

 private:
  boost::scoped_ptr<uint32_t> pixel_buffer_;
  pp::Size pixel_buffer_size_;
  int32_t row_bytes_;

  // Not implemented, do not use.
  Sprite();
};

}  // namespace flocking_geese

#endif  // SPRITE_H_
