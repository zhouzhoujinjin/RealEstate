import { DatePicker, Form, Input, Select } from 'antd'
import React from 'react'

export const EditableCell = ({
  editing,
  dataIndex,
  inputType,
  children,
  options,
  ...restProps
}) => {
  let inputNode
  switch (inputType) {
    case 'input':
      inputNode = <Input />
      break
    case 'textarea':
      inputNode = <Input.TextArea />
      break
    case 'select':
      inputNode = (
        <Select style={{ width: '100%' }}>
          {options?.map((x) => (
            <Select.Option key={x.id} value={x.id}>
              {x.label}
            </Select.Option>
          ))}
        </Select>
      )
      break
    case 'datepicker':
      inputNode = <DatePicker />
      break
    case 'datetimepicker':
      inputNode = <DatePicker showTime />
      break

    default:
      return <td {...restProps}>{children}</td>
  }
  return (
    <td {...restProps}>
      {editing ? (
        <Form.Item
          noStyle
          name={dataIndex}
          style={{
            margin: 0,
          }}
        >
          {inputNode}
        </Form.Item>
      ) : (
        children
      )}
    </td>
  )
}
